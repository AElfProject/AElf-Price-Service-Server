using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EsRepository;
using Microsoft.Extensions.Logging;
using Nest;
using TokenPriceUpdate.Helpers;
using TokenPriceUpdate.Price;
using TokenPriceUpdate.Price.DTOs;
using TokenPriceUpdate.Price.Entities.Es;
using TokenPriceUpdate.Providers;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;

namespace TokenPriceUpdate
{
    [RemoteService(IsEnabled = false)]
    public class PriceAppService : ApplicationService, IPriceAppService
    {
        private readonly Volo.Abp.Domain.Repositories.IRepository<Price.Entities.Ef.Price> _priceRepository;
        private readonly INESTRepository<Price.Entities.Es.Price, Guid> _esPriceRepository;
        private readonly INESTRepository<PriceRecord, Guid> _esPriceRecordRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ISwapTokenPriceQueryIndexProvider _swapTokenPriceQueryIndexProvider;
        private readonly ICachedPriceProvider _cachedPriceProvider;
        private readonly ILogger<PriceAppService> _logger;

        public PriceAppService(Volo.Abp.Domain.Repositories.IRepository<Price.Entities.Ef.Price> priceRepository,
            INESTRepository<Price.Entities.Es.Price, Guid> esPriceRepository,
            INESTRepository<PriceRecord, Guid> esPriceRecordRepository, IGuidGenerator guidGenerator,
            ISwapTokenPriceQueryIndexProvider swapTokenPriceQueryIndexProvider, ILogger<PriceAppService> logger,
            ICachedPriceProvider cachedPriceProvider)
        {
            _priceRepository = priceRepository;
            _esPriceRepository = esPriceRepository;
            _esPriceRecordRepository = esPriceRecordRepository;
            _guidGenerator = guidGenerator;
            _swapTokenPriceQueryIndexProvider = swapTokenPriceQueryIndexProvider;
            _cachedPriceProvider = cachedPriceProvider;
            _logger = logger;
        }

        public async Task CreateOrUpdateTokenPriceAsync(CreateOrUpdateTokenPriceInput input)
        {
            var (_, token, underlyingToken, price) =
                GetAdjustTokenPriceInfo(input.Token,
                    input.UnderlyingToken, out _, input.Price);
            var priceInfo = await _priceRepository.FindAsync(x =>
                x.TokenSymbol == token && x.UnderlyingTokenSymbol == underlyingToken && x.PriceType == input.PriceType);
            if (priceInfo == null)
            {
                await _priceRepository.InsertAsync(new Price.Entities.Ef.Price
                {
                    TokenSymbol = token,
                    UnderlyingTokenSymbol = underlyingToken,
                    Price = price,
                    DateTime = input.DateTime,
                    PriceType = input.PriceType
                });
                return;
            }

            if (priceInfo.DateTime > input.DateTime)
            {
                return;
            }

            priceInfo.Price = price;
            priceInfo.DateTime = input.DateTime;
            await _priceRepository.UpdateAsync(priceInfo);
        }

        public async Task CreateOrUpdateTokenPriceOnEsAsync(CreateOrUpdateTokenPriceOnEsInput input)
        {
            var (tokenKey, token, underlyingToken, price) =
                GetAdjustTokenPriceInfo(input.Token,
                    input.UnderlyingToken, out _, input.Price);
            var existedPrice = await _esPriceRepository.GetAsync(input.Id);
            if (existedPrice != null && existedPrice.DateTime >= input.DateTime)
            {
                existedPrice.Id = _guidGenerator.Create();
                existedPrice.DateTime = input.DateTime;
                existedPrice.Price = price;
                await AddPriceRecordAsync(existedPrice);
                return;
            }

            var priceInfo = new Price.Entities.Es.Price
            {
                Id = input.Id,
                TokenKey = tokenKey,
                TokenSymbol = token,
                UnderlyingTokenSymbol = underlyingToken,
                DateTime = input.DateTime,
                Price = price,
                PriceType = input.PriceType
            };
            await _esPriceRepository.AddOrUpdateAsync(priceInfo);
            priceInfo.Id = _guidGenerator.Create();
            await AddPriceRecordAsync(priceInfo);
        }

        private async Task AddPriceRecordAsync(Price.Entities.Es.Price price)
        {
            var record = await _esPriceRecordRepository.GetAsync(
                GetPriceRecordFilterQueryContainer(new GetTokenPriceRecordInput
                {
                    StartTime = DateTimeHelper.ToUnixTimeMilliseconds(price.DateTime),
                    EndTime = DateTimeHelper.ToUnixTimeMilliseconds(price.DateTime)
                }, price.TokenKey, price.PriceType));
            if (record != null)
            {
                return;
            }
            
            await _esPriceRecordRepository.AddAsync(
                ObjectMapper.Map<Price.Entities.Es.Price, PriceRecord>(price));
        }

        public async Task<CurrentTokenPriceDto> GetSwapTokenPriceAsync(GetTokenPriceInput input)
        {
            var tokenQueryList =
                _swapTokenPriceQueryIndexProvider.GetTokenPriceIndex(input.Token, input.UnderlyingToken);
            var uncachedTokenPair = await GetUncachedTokenPairAsync(tokenQueryList);
            if (uncachedTokenPair.Any())
            {
                await BatchGetAndCachePriceAsync(new BatchGetTokenPriceInput
                {
                    TokenInputs = uncachedTokenPair.Select(x => new GetTokenPriceInput
                    {
                        Token = x.Item1,
                        UnderlyingToken = x.Item2
                    }).ToArray()
                }, PriceType.FromSwapToken);
            }

            var price = await CalculatePriceAsync(tokenQueryList);
            return new CurrentTokenPriceDto
            {
                Token = input.Token,
                UnderlyingToken = input.UnderlyingToken,
                Price = price
            };
        }

        public async Task<CurrentTokenPriceDto> GetExchangePriceAsync(GetTokenPriceInput input)
        {
            var tokenKey =
                GetTokenKey(input.Token, input.UnderlyingToken, out var isAdjustOrder);
            var pairPrice = await _cachedPriceProvider.GetExchangeTokenPriceAsync(tokenKey);
            if (pairPrice < 0m)
            {
                return await GetAndCachePriceAsync(input.Token, input.UnderlyingToken,
                    PriceType.FromExchange);
            }

            pairPrice = GetModifiedPrice(pairPrice, isAdjustOrder);
            return new CurrentTokenPriceDto
            {
                Token = input.Token,
                UnderlyingToken = input.UnderlyingToken,
                Price = pairPrice
            };
        }

        public async Task<ListResultDto<CurrentTokenPriceDto>> BatchGetSwapTokenPriceAsync(
            BatchGetTokenPriceInput input)
        {
            var tokenQueryList = new List<(string, string)>();
            foreach (var tokenPair in input.TokenInputs)
            {
                tokenQueryList = tokenQueryList.Union(
                    _swapTokenPriceQueryIndexProvider.GetTokenPriceIndex(tokenPair.Token,
                        tokenPair.UnderlyingToken)).ToList();
            }

            var uncachedTokenPair = (await GetUncachedTokenPairAsync(tokenQueryList)).Select(x => new GetTokenPriceInput
            {
                Token = x.Item1,
                UnderlyingToken = x.Item2
            }).ToArray();
            if (uncachedTokenPair.Any())
            {
                await BatchGetAndCachePriceAsync(new BatchGetTokenPriceInput
                {
                    TokenInputs = uncachedTokenPair
                }, PriceType.FromSwapToken);
            }

            var tokenPriceDto = new List<CurrentTokenPriceDto>();
            foreach (var tokenPair in input.TokenInputs)
            {
                tokenQueryList =
                    _swapTokenPriceQueryIndexProvider.GetTokenPriceIndex(tokenPair.Token,
                        tokenPair.UnderlyingToken);
                var price = await CalculatePriceAsync(tokenQueryList);
                tokenPriceDto.Add(new CurrentTokenPriceDto
                {
                    Token = tokenPair.Token,
                    UnderlyingToken = tokenPair.UnderlyingToken,
                    Price = price
                });
            }

            return new ListResultDto<CurrentTokenPriceDto>
            {
                Items = tokenPriceDto
            };
        }

        private async Task<decimal> CalculatePriceAsync(List<(string, string)> tokenQueryList)
        {
            if (!tokenQueryList.Any())
            {
                return 0m;
            }

            var price = 1m;
            foreach (var tokenPair in tokenQueryList)
            {
                var tokenKey = GetTokenKey(tokenPair.Item1, tokenPair.Item2, out var isAdjustOrder);
                var pairPrice = await _cachedPriceProvider.GetSwapTokenPriceAsync(tokenKey);
                if (pairPrice < 0m)
                {
                    _logger.LogWarning($"Lack off token pair price: {tokenKey}");
                    return 0m;
                }

                pairPrice = GetModifiedPrice(pairPrice, isAdjustOrder);
                price *= pairPrice;
            }

            return price;
        }

        public async Task<ListResultDto<CurrentTokenPriceDto>> BatchGetExchangePriceAsync(BatchGetTokenPriceInput input)
        {
            var priceList = new List<CurrentTokenPriceDto>();
            var unCachedInput = new List<GetTokenPriceInput>();
            foreach (var tokenInput in input.TokenInputs)
            {
                var tokenKey = GetTokenKey(tokenInput.Token, tokenInput.UnderlyingToken,
                    out var isChangeOrder);
                var cachedPrice = await _cachedPriceProvider.GetExchangeTokenPriceAsync(tokenKey);
                if (cachedPrice < 0m)
                {
                    unCachedInput.Add(tokenInput);
                    continue;
                }

                if (isChangeOrder)
                {
                    cachedPrice = GetPriceReciprocal(cachedPrice);
                }

                priceList.Add(new CurrentTokenPriceDto
                {
                    Token = tokenInput.Token,
                    UnderlyingToken = tokenInput.UnderlyingToken,
                    Price = cachedPrice
                });
            }

            if (unCachedInput.Any())
            {
                priceList.AddRange(await BatchGetAndCachePriceAsync(new BatchGetTokenPriceInput
                {
                    TokenInputs = unCachedInput.ToArray()
                }, PriceType.FromExchange));
            }

            priceList = (from r in input.TokenInputs
                join cr in priceList
                    on r.Token + r.UnderlyingToken equals cr.Token + cr.UnderlyingToken into tt
                from cr in tt.DefaultIfEmpty()
                select new CurrentTokenPriceDto
                {
                    Token = r.Token,
                    UnderlyingToken = r.UnderlyingToken,
                    Price = cr?.Price ?? 0m
                }).ToList();

            return new ListResultDto<CurrentTokenPriceDto>
            {
                Items = priceList
            };
        }

        public async Task<TokenPriceDto> GetSwapTokenPriceByDatetimeAsync(GetTokenPriceByDatetimeInput input)
        {
            var tokenQueryIndex =
                _swapTokenPriceQueryIndexProvider.GetTokenPriceIndex(input.Token,
                    input.UnderlyingToken);
            var tokenPriceDto = new TokenPriceDto(input.Token, input.UnderlyingToken);
            var price = 1m;
            foreach (var (item1, item2) in tokenQueryIndex)
            {
                var tokenPriceInfo = await GetPriceByDatetimeAsync(
                    new GetTokenPriceByDatetimeInput(item1, item2, input.Time),
                    PriceType.FromSwapToken);
                if (tokenPriceInfo.Price == 0m)
                {
                    return tokenPriceDto;
                }

                if (tokenPriceInfo.DateTime > tokenPriceDto.DateTime)
                {
                    tokenPriceDto.DateTime = tokenPriceInfo.DateTime;
                }

                price *= tokenPriceInfo.Price;
            }

            tokenPriceDto.Price = GetPrice(price);
            return tokenPriceDto;
        }

        public async Task<TokenPriceDto> GetExchangePriceByDatetimeAsync(GetTokenPriceByDatetimeInput input)
        {
            return await GetPriceByDatetimeAsync(input, PriceType.FromExchange);
        }

        public void UpdateQueryIndexAndCachePrice(UpdateQueryIndexAndCachePriceInput input)
        {
            var (tokenKey, token, underlyingToken, price) =
                GetAdjustTokenPriceInfo(input.Token, input.UnderlyingToken, out _, input.Price);

            if (input.PriceType == PriceType.FromSwapToken)
            {
                _swapTokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token, underlyingToken);
            }

            UpdatePriceCache(input.PriceType, tokenKey, price, input.DateTime);
        }

        public async Task<TokenPriceRecordDto> GetSwapTokenPriceRecordsAsync(
            GetTokenPriceRecordInput input)
        {
            return await GetPriceRecordsAsync(input, PriceType.FromSwapToken);
        }

        public async Task<TokenPriceRecordDto> GetExchangePriceRecordsAsync(
            GetTokenPriceRecordInput input)
        {
            return await GetPriceRecordsAsync(input, PriceType.FromExchange);
        }

        private void UpdatePriceCache(PriceType priceType, string tokenKey, decimal price, DateTime dateTime)
        {
            switch (priceType)
            {
                case PriceType.FromSwapToken:
                    _cachedPriceProvider.SetSwapTokenPriceAsync(tokenKey, price, dateTime);
                    return;
                case PriceType.FromExchange:
                    _cachedPriceProvider.SetExchangeTokenPriceAsync(tokenKey, price, dateTime);
                    return;
                default:
                    throw new Exception($"Invalid price type: {priceType} for token key:{tokenKey}");
            }
        }


        private async Task<List<(string, string)>> GetUncachedTokenPairAsync(IEnumerable<(string, string)> tokenPairs)
        {
            var unCachedTokenPairs = new List<(string, string)>();
            foreach (var tokenPair in tokenPairs)
            {
                if (!await _cachedPriceProvider.IsCachedSwapPriceAsync(GetTokenKey(tokenPair.Item1, tokenPair.Item2,
                    out _)))
                {
                    unCachedTokenPairs.Add(tokenPair);
                }
            }

            return unCachedTokenPairs;
        }

        private async Task<TokenPriceDto> GetAndCachePriceAsync(string token, string underlyingToken,
            PriceType priceType)
        {
            var tokenKey =
                GetTokenKey(token, underlyingToken, out var isAdjustOrder);
            var tokenPriceInfo =
                await _esPriceRepository.GetAsync(GetPriceFilterQueryContainer(tokenKey, priceType));
            var tokenPriceDto = new TokenPriceDto(token, underlyingToken);
            if (tokenPriceInfo == null)
            {
                return tokenPriceDto;
            }

            UpdatePriceCache(priceType, tokenKey, tokenPriceInfo.Price, tokenPriceInfo.DateTime);
            tokenPriceDto.Price = GetModifiedPrice(tokenPriceInfo.Price, isAdjustOrder);
            tokenPriceDto.DateTime = tokenPriceInfo.DateTime;
            return tokenPriceDto;
        }

        private Func<QueryContainerDescriptor<Price.Entities.Es.Price>, QueryContainer> GetPriceFilterQueryContainer(
            string tokenKey, PriceType priceType)
        {
            return q =>
            {
                return q
                           .Term(t => t
                               .Field(f => f.TokenKey).Value(tokenKey)) &&
                       q
                           .Term(t => t
                               .Field(f => f.PriceType).Value(priceType));
            };
        }

        private async Task<List<CurrentTokenPriceDto>> BatchGetAndCachePriceAsync(
            BatchGetTokenPriceInput input,
            PriceType priceType)
        {
            if (input.TokenInputs == null || !input.TokenInputs.Any())
            {
                return new List<CurrentTokenPriceDto>();
            }

            var (_, tokenPrices) =
                await _esPriceRepository.GetListAsync(
                    GetBatchPriceFilterQueryContainer(input.TokenInputs, priceType));
            var tokenPriceDto = new List<CurrentTokenPriceDto>();
            foreach (var tokenInput in input.TokenInputs)
            {
                var tokenKey = GetTokenKey(tokenInput.Token, tokenInput.UnderlyingToken,
                    out var isChangeOrder);
                var targetTokenPriceInfo = tokenPrices.SingleOrDefault(x => x.TokenKey == tokenKey);
                if (targetTokenPriceInfo == null)
                {
                    continue;
                }

                UpdatePriceCache(priceType, tokenKey, targetTokenPriceInfo.Price, targetTokenPriceInfo.DateTime);
                var price = isChangeOrder ? GetPriceReciprocal(targetTokenPriceInfo.Price) : targetTokenPriceInfo.Price;
                tokenPriceDto.Add(new CurrentTokenPriceDto
                {
                    Token = tokenInput.Token,
                    UnderlyingToken = tokenInput.UnderlyingToken,
                    Price = price
                });
            }

            return tokenPriceDto;
        }

        private Func<QueryContainerDescriptor<Price.Entities.Es.Price>, QueryContainer>
            GetBatchPriceFilterQueryContainer(
                GetTokenPriceInput[] tokensInfo, PriceType priceType)
        {
            var tokenKeysList = tokensInfo.Select(x => GetTokenKey(x.Token, x.UnderlyingToken, out _))
                .ToList();
            return q =>
            {
                return q
                           .Terms(t => t
                               .Field(f => f.TokenKey).Terms(tokenKeysList)) &&
                       q
                           .Term(t => t
                               .Field(f => f.PriceType).Value(priceType));
            };
        }

        private async Task<TokenPriceRecordDto> GetPriceRecordsAsync(GetTokenPriceRecordInput input,
            PriceType priceType)
        {
            var skipCount = input.SkipCount > 0 ? input.SkipCount : 0;
            var size = input.Size > 0 ? input.Size : PriceConstants.DefaultRecordSize;
            var tokenKey = GetTokenKey(input.Token, input.UnderlyingToken, out var isChangeOrder);
            var totalCountInfo =
                await _esPriceRecordRepository.CountAsync(
                    GetPriceRecordFilterQueryContainer(input, tokenKey, priceType));
            var tokenPriceRecordDto = new TokenPriceRecordDto
            {
                Token = input.Token,
                UnderlyingToken = input.UnderlyingToken,
                TotalCount = totalCountInfo.Count
            };

            if (totalCountInfo.Count == 0)
            {
                return tokenPriceRecordDto;
            }

            var (_, records) = await _esPriceRecordRepository.GetListAsync(
                GetPriceRecordFilterQueryContainer(input, tokenKey, priceType), null, x => x.DateTime,
                SortOrder.Ascending, size, skipCount);
            foreach (var priceInfo in records.Select(record => new PriceInfoDto
            {
                DateTime = record.DateTime,
                Price = isChangeOrder ? GetPriceReciprocal(record.Price) : record.Price
            }))
            {
                tokenPriceRecordDto.PriceInfoList.Add(priceInfo);
            }

            return tokenPriceRecordDto;
        }

        private Func<QueryContainerDescriptor<PriceRecord>, QueryContainer> GetPriceRecordFilterQueryContainer(
            GetTokenPriceRecordInput input, string tokenKey, PriceType priceType)
        {
            return q =>
            {
                var totalQueryContainer = q
                                              .Term(t => t
                                                  .Field(f => f.TokenKey).Value(tokenKey)) &&
                                          q
                                              .Term(t => t
                                                  .Field(f => f.PriceType).Value(priceType));
                if (input.StartTime > 0)
                {
                    var startTimeDate = DateTimeHelper.FromUnixTimeMilliseconds(input.StartTime);
                    totalQueryContainer = totalQueryContainer && +q
                        .DateRange(r => r
                            .Field(f => f.DateTime)
                            .GreaterThanOrEquals(startTimeDate));
                }

                if (input.EndTime <= 0)
                {
                    return totalQueryContainer;
                }

                {
                    var endTimeDate = DateTimeHelper.FromUnixTimeMilliseconds(input.EndTime);
                    totalQueryContainer = totalQueryContainer && +q
                        .DateRange(r => r
                            .Field(f => f.DateTime)
                            .LessThanOrEquals(endTimeDate));
                }

                return totalQueryContainer;
            };
        }

        private async Task<TokenPriceDto> GetPriceByDatetimeAsync(GetTokenPriceByDatetimeInput byDatetimeInput,
            PriceType priceType)
        {
            var tokenPriceDto = new TokenPriceDto(byDatetimeInput.Token, byDatetimeInput.UnderlyingToken);
            if (byDatetimeInput.Time <= 0)
            {
                return tokenPriceDto;
            }

            var tokenKey = GetTokenKey(byDatetimeInput.Token, byDatetimeInput.UnderlyingToken, out var isAdjustOrder);
            var priceRecord =
                await _esPriceRecordRepository.GetAsync(
                    GetHistoryPriceRecordFilterQueryContainer(byDatetimeInput.Time, tokenKey, priceType), null,
                    x => x.DateTime, SortOrder.Descending);
            if (priceRecord == null)
            {
                return tokenPriceDto;
            }

            tokenPriceDto.Price = GetModifiedPrice(priceRecord.Price, isAdjustOrder);
            tokenPriceDto.DateTime = priceRecord.DateTime;
            return tokenPriceDto;
        }

        private Func<QueryContainerDescriptor<PriceRecord>, QueryContainer> GetHistoryPriceRecordFilterQueryContainer(
            long time, string tokenKey, PriceType priceType)
        {
            var startTimeDate = DateTimeHelper.FromUnixTimeMilliseconds(time);
            return q =>
            {
                return q
                           .Term(t => t
                               .Field(f => f.TokenKey).Value(tokenKey)) &&
                       q
                           .Term(t => t
                               .Field(f => f.PriceType).Value(priceType)) && +q
                           .DateRange(r => r
                               .Field(f => f.DateTime)
                               .LessThanOrEquals(startTimeDate));
            };
        }

        public static (string, string, string, decimal) GetAdjustTokenPriceInfo(string originalToken,
            string underlyingToken, out bool isAdjustOrder, decimal price = 1)
        {
            var tokenKey = GetTokenKey(originalToken, underlyingToken, out isAdjustOrder);
            if (!isAdjustOrder)
            {
                return (tokenKey, originalToken, underlyingToken, price);
            }

            price = GetPriceReciprocal(price);
            return (tokenKey, underlyingToken, originalToken, price);
        }

        private static decimal GetPrice(decimal price)
        {
            return decimal.Round(price, PriceConstants.PriceDecimal);
        }

        private static decimal GetPriceReciprocal(decimal price)
        {
            return decimal.Round(1 / price, PriceConstants.PriceDecimal);
        }

        private static string GetTokenKey(string token1, string token2, out bool isAdjustOrder)
        {
            isAdjustOrder = IsAdjustTokenOrder(token1, token2);
            return !isAdjustOrder ? $"{token1}-{token2}" : $"{token2}-{token1}";
        }

        private static bool IsAdjustTokenOrder(string token1, string token2)
        {
            var comparision = string.Compare(token1, token2, StringComparison.Ordinal);
            return comparision < 0;
        }

        private static decimal GetModifiedPrice(decimal price, bool isAdjustOrder)
        {
            return isAdjustOrder ? GetPriceReciprocal(price) : GetPrice(price);
        }
    }
}