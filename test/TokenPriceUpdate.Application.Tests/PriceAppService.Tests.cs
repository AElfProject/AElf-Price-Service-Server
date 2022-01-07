using System;
using System.Threading.Tasks;
using Shouldly;
using TokenPriceUpdate.Helpers;
using TokenPriceUpdate.Price;
using TokenPriceUpdate.Price.DTOs;
using TokenPriceUpdate.Providers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace TokenPriceUpdate
{
    public class PriceAppServiceTests : TokenPriceUpdateApplicationTestBase
    {
        private readonly IPriceAppService _priceAppService;
        private readonly ICachedPriceProvider _cachedPriceProvider;
        private readonly IRepository<Price.Entities.Ef.Price> _priceRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly string[] _underlyingBaseTokens;

        public PriceAppServiceTests()
        {
            _priceAppService = GetRequiredService<IPriceAppService>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
            _priceRepository = GetRequiredService<IRepository<Price.Entities.Ef.Price>>();
            _cachedPriceProvider = GetRequiredService<ICachedPriceProvider>();
            _underlyingBaseTokens = TokenNode.RootTokenNodeNames;
        }

        [Fact]
        public async Task CreateOrUpdateTokenPrice_In_Db_Create_Should_Get_Right_Data()
        {
            var tokenOne = "JWC";
            var tokenTwo = "TJU";
            var price = 1m;
            var dateTime = DateTime.UtcNow;
            await CreatePriceInfoInDbAsync(PriceType.FromSwapToken, tokenOne, tokenTwo, price, dateTime);
            var priceInfo = await _priceRepository.FindAsync(x =>
                x.TokenSymbol == tokenTwo && x.UnderlyingTokenSymbol == tokenOne &&
                x.PriceType == PriceType.FromSwapToken) ?? await _priceRepository.FindAsync(x =>
                x.TokenSymbol == tokenOne && x.UnderlyingTokenSymbol == tokenTwo &&
                x.PriceType == PriceType.FromSwapToken);
            priceInfo.ShouldNotBeNull();
            priceInfo.DateTime.ShouldBe(dateTime);
        }

        [Fact]
        public async Task CreateOrUpdateTokenPrice_In_Db_Update_Should_Get_Right_Data()
        {
            var tokenOne = "YP";
            var tokenTwo = "PY";
            var price = 1m;
            var dateTime = DateTime.UtcNow;
            await CreatePriceInfoInDbAsync(PriceType.FromSwapToken, tokenOne, tokenTwo, price, dateTime);
            price = 2m;
            dateTime = dateTime.AddSeconds(1);
            await CreatePriceInfoInDbAsync(PriceType.FromSwapToken, tokenOne, tokenTwo, price, dateTime);
            var priceInfo = await _priceRepository.FindAsync(x =>
                x.TokenSymbol == tokenTwo && x.UnderlyingTokenSymbol == tokenOne &&
                x.PriceType == PriceType.FromSwapToken);
            if (priceInfo == null)
            {
                priceInfo = await _priceRepository.FindAsync(x =>
                    x.TokenSymbol == tokenOne && x.UnderlyingTokenSymbol == tokenTwo &&
                    x.PriceType == PriceType.FromSwapToken);
                priceInfo.ShouldNotBeNull();
                priceInfo.DateTime.ShouldBe(dateTime);
                priceInfo.Price.ShouldBe(price);
            }
            else
            {
                priceInfo.ShouldNotBeNull();
                priceInfo.DateTime.ShouldBe(dateTime);
                priceInfo.Price.ShouldBe(1 / price);
            }
        }

        [Fact]
        public async Task CreateOrUpdateTokenPrice_Create_And_Update_Price_Should_Get_Right_Data()
        {
            var tokenOne = "WLL";
            var tokenTwo = "JUNE";
            var price = 1m;
            var dateTime = DateTime.UtcNow;
            var priceId = await CreatePriceInfoOnEsAsync(PriceType.FromSwapToken, tokenOne, tokenTwo, price, dateTime);
            price = 2m;
            dateTime = dateTime.AddSeconds(1);
            await CreatePriceInfoOnEsAsync(PriceType.FromSwapToken, tokenOne, tokenTwo, price, dateTime, true, priceId);
            var currentPriceInfo =
                await _priceAppService.GetSwapTokenPriceAsync(new GetTokenPriceInput(tokenOne, tokenTwo));
            currentPriceInfo.Token.ShouldBe(tokenOne);
            currentPriceInfo.UnderlyingToken.ShouldBe(tokenTwo);
            currentPriceInfo.Price.ShouldBe(price);

            priceId = await CreatePriceInfoOnEsAsync(PriceType.FromExchange, tokenOne, tokenTwo, price, dateTime);
            currentPriceInfo =
                await _priceAppService.GetExchangePriceAsync(new GetTokenPriceInput(tokenOne, tokenTwo));
            currentPriceInfo.Token.ShouldBe(tokenOne);
            currentPriceInfo.UnderlyingToken.ShouldBe(tokenTwo);
            currentPriceInfo.Price.ShouldBe(price);

            price = 3m;
            dateTime = dateTime.AddSeconds(1);
            await CreatePriceInfoOnEsAsync(PriceType.FromExchange, tokenOne, tokenTwo, price, dateTime, true, priceId);
            currentPriceInfo =
                await _priceAppService.GetExchangePriceAsync(new GetTokenPriceInput(tokenOne, tokenTwo));
            currentPriceInfo.Token.ShouldBe(tokenOne);
            currentPriceInfo.UnderlyingToken.ShouldBe(tokenTwo);
            currentPriceInfo.Price.ShouldBe(price);
        }

        [Fact]
        public async Task BatchGetSwapTokenPriceAsync_Should_Get_Right_Data()
        {
            await InitializeTokenPairPriceAsync(PriceType.FromSwapToken);
            var priceList = (await _priceAppService.BatchGetSwapTokenPriceAsync(new BatchGetTokenPriceInput
            {
                TokenInputs = new[]
                {
                    new GetTokenPriceInput("QM", "PY"),
                    new GetTokenPriceInput("QM", "USDT"),
                    new GetTokenPriceInput("PY", "USDT"),
                    new GetTokenPriceInput("WJC", "USDT"),
                    new GetTokenPriceInput("ZX", "USDT"),
                    new GetTokenPriceInput("YX", "USDT"),
                    new GetTokenPriceInput("YX", "WJC"),
                    new GetTokenPriceInput("USDT", "YX")
                }
            })).Items;
            priceList.Count.ShouldBe(8);
            priceList[0].Token.ShouldBe("QM");
            priceList[0].UnderlyingToken.ShouldBe("PY");
            priceList[0].Price.ShouldBe(1m);
            priceList[1].Price.ShouldBe(0m);
            priceList[2].Price.ShouldBe(40m);
            priceList[3].Price.ShouldBe(50m);
            priceList[4].Price.ShouldBe(10m);
            priceList[5].Price.ShouldBe(16m);
            priceList[6].Price.ShouldBe(0.125m);
            priceList[7].Price.ShouldBe(0.0625m);
        }

        [Fact]
        public async Task BatchGetExchange_PriceAsync_Should_Get_Right_Data()
        {
            await InitializeTokenPairPriceAsync(PriceType.FromExchange);
            var priceList = (await _priceAppService.BatchGetExchangePriceAsync(new BatchGetTokenPriceInput
            {
                TokenInputs = new[]
                {
                    new GetTokenPriceInput("QM", "PY"),
                    new GetTokenPriceInput("QM", "USDT"),
                    new GetTokenPriceInput("PY", "USDT"),
                    new GetTokenPriceInput("WJC", "USDT"),
                    new GetTokenPriceInput("ZX", "USDT"),
                    new GetTokenPriceInput("YX", "USDT"),
                    new GetTokenPriceInput("YX", "WJC"),
                    new GetTokenPriceInput("USDT", "YX")
                }
            })).Items;
            priceList.Count.ShouldBe(8);
            priceList[0].Token.ShouldBe("QM");
            priceList[0].UnderlyingToken.ShouldBe("PY");
            priceList[0].Price.ShouldBe(1m);
            priceList[1].Price.ShouldBe(0m);
            priceList[2].Price.ShouldBe(0m);
            priceList[3].Price.ShouldBe(0m);
            priceList[4].Price.ShouldBe(10m);
            priceList[5].Price.ShouldBe(16m);
            priceList[6].Price.ShouldBe(0.125m);
            priceList[7].Price.ShouldBe(0.0625m);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetPriceRecordsAsync_Should_Get_Right_Data(int priceTypeInteger)
        {
            var priceType = (PriceType) priceTypeInteger;
            var token1 = "QM";
            var token2 = "PY";
            var dateTime1 = DateTime.UtcNow;
            var price = 1m;
            await CreatePriceInfoOnEsAsync(priceType, token1, token2, price, dateTime1);

            var records =
                 await GetRecordByPriceTypeAsync(new GetTokenPriceRecordInput(token1, token2), priceType);
            records.TotalCount.ShouldBe(1);
            records.PriceInfoList.Count.ShouldBe(1);
            records.PriceInfoList[0].DateTime.ShouldBe(dateTime1);
            records.PriceInfoList[0].Price.ShouldBe(1m);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetPriceRecordsAsync_With_Start_And_End_Time_Should_Get_Right_Data(int priceTypeInteger)
        {
            var priceType = (PriceType) priceTypeInteger;
            var token1 = "QM";
            var token2 = "PY";
            var dateTime1 = DateTime.UtcNow;
            var price1 = 1m;
            await CreatePriceInfoOnEsAsync(priceType, token1, token2, price1, dateTime1);
            var dateTime2 = dateTime1.AddHours(1);
            var price2 = 2m;
            await CreatePriceInfoOnEsAsync(priceType, token1, token2, price2, dateTime2);
            var records = await GetRecordByPriceTypeAsync(new GetTokenPriceRecordInput(token1, token2)
            {
                StartTime = DateTimeHelper.ToUnixTimeMilliseconds(dateTime1.AddHours(-1)),
                EndTime = DateTimeHelper.ToUnixTimeMilliseconds(dateTime2.AddSeconds(-1))
            }, priceType);
            records.TotalCount.ShouldBe(1);
            records.PriceInfoList.Count.ShouldBe(1);
            records.PriceInfoList[0].DateTime.ShouldBe(dateTime1);
            records.PriceInfoList[0].Price.ShouldBe(1m);
            
            records = await GetRecordByPriceTypeAsync(new GetTokenPriceRecordInput(token1, token2)
            {
                EndTime = DateTimeHelper.ToUnixTimeMilliseconds(dateTime1.AddSeconds(-1))
            }, priceType);
            records.TotalCount.ShouldBe(0);
            
            records = await GetRecordByPriceTypeAsync(new GetTokenPriceRecordInput(token1, token2)
            {
                StartTime = DateTimeHelper.ToUnixTimeMilliseconds(dateTime2.AddSeconds(1))
            }, priceType);
            records.TotalCount.ShouldBe(0);
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetSwapTokenPriceByDateTimeAsync_Should_Get_Right_Data(int priceTypeInteger)
        {
            var priceType = (PriceType) priceTypeInteger;
            var token1 = "QM";
            var token2 = "PY";
            var dateTime = DateTime.UtcNow;
            var price = 1m;
            await CreatePriceInfoOnEsAsync(priceType, token1, token2, price, dateTime);
            var queryDatetime = dateTime.AddHours(-1);
            var timestamp = DateTimeHelper.ToUnixTimeMilliseconds(queryDatetime);
            var priceAtDateTime = await
                GetRecordByDatetimeAndPriceTypeAsync(new GetTokenPriceByDatetimeInput(token1, token2, timestamp),
                    priceType);
            priceAtDateTime.DateTime.ShouldBe(new DateTime());
            priceAtDateTime.Price.ShouldBe(0m);

            queryDatetime = dateTime.AddHours(1);
            timestamp = DateTimeHelper.ToUnixTimeMilliseconds(queryDatetime);
            priceAtDateTime = await
                GetRecordByDatetimeAndPriceTypeAsync(new GetTokenPriceByDatetimeInput(token1, token2, timestamp),
                    priceType);
            priceAtDateTime.DateTime.ShouldBe(dateTime);
            priceAtDateTime.Price.ShouldBe(1m);
        }

        //     p1
        // QM  —— >  PY
        // | p2      | p3
        // v    p4   v
        // WJC —— > ZX
        // | p5      | p6 
        // v    p7   v
        // YX —— > USDT
        private async Task InitializeTokenPairPriceAsync(PriceType priceType)
        {
            var token1 = "QM";
            var token2 = "PY";
            var token3 = "ZX";
            var token4 = "WJC";
            var token5 = "YX";
            var token6 = _underlyingBaseTokens[1]; // USDT
            var price1 = 1m;
            var price2 = 2m;
            var price3 = 4m;
            var price4 = 5m;
            var price5 = 8m;
            var price6 = 10m;
            var price7 = 16m;
            var dateTime = DateTime.UtcNow;

            await CreatePriceInfoOnEsAsync(priceType, token1, token2, price1, dateTime, true);
            dateTime = dateTime.AddHours(1);
            await CreatePriceInfoOnEsAsync(priceType, token1, token4, price2, dateTime, true);
            dateTime = dateTime.AddHours(1);
            await CreatePriceInfoOnEsAsync(priceType, token2, token3, price3, dateTime, true);
            dateTime = dateTime.AddHours(1);
            await CreatePriceInfoOnEsAsync(priceType, token4, token3, price4, dateTime, true);
            dateTime = dateTime.AddHours(1);
            await CreatePriceInfoOnEsAsync(priceType, token4, token5, price5, dateTime);
            dateTime = dateTime.AddHours(1);
            await CreatePriceInfoOnEsAsync(priceType, token3, token6, price6, dateTime);
            dateTime = dateTime.AddHours(1);
            await CreatePriceInfoOnEsAsync(priceType, token5, token6, price7, dateTime);
        }

        private async Task CreatePriceInfoInDbAsync(PriceType priceType, string token, string underlyingToken,
            decimal price, DateTime dateTime)
        {
            await _priceAppService.CreateOrUpdateTokenPriceAsync(new CreateOrUpdateTokenPriceInput
            {
                Token = token,
                UnderlyingToken = underlyingToken,
                Price = price,
                PriceType = priceType,
                DateTime = dateTime
            });
        }

        private async Task<Guid> CreatePriceInfoOnEsAsync(PriceType priceType, string token, string underlyingToken,
            decimal price, DateTime dateTime, bool isClearCache = false, Guid? priceId = null)
        {
            priceId ??= _guidGenerator.Create();
            await _priceAppService.CreateOrUpdateTokenPriceOnEsAsync(new CreateOrUpdateTokenPriceOnEsInput
            {
                Id = priceId.Value,
                Token = token,
                UnderlyingToken = underlyingToken,
                Price = price,
                PriceType = priceType,
                DateTime = dateTime
            });

            _priceAppService.UpdateQueryIndexAndCachePrice(new UpdateQueryIndexAndCachePriceInput
            {
                Token = token,
                UnderlyingToken = underlyingToken,
                Price = price,
                PriceType = priceType,
                DateTime = dateTime
            });

            if (!isClearCache)
            {
                return priceId.Value;
            }

            var tokenKey = $"{token}-{underlyingToken}";
            await _cachedPriceProvider.ClearCacheAsync(tokenKey);
            tokenKey = $"{underlyingToken}-{token}";
            await _cachedPriceProvider.ClearCacheAsync(tokenKey);
            return priceId.Value;
        }

        private async Task<TokenPriceRecordDto> GetRecordByPriceTypeAsync(GetTokenPriceRecordInput input,
            PriceType priceType)
        {
            if (priceType == PriceType.FromSwapToken)
            {
                return await _priceAppService.GetSwapTokenPriceRecordsAsync(input);
            }

            return await _priceAppService.GetExchangePriceRecordsAsync(input);
        }
        
        private async Task<TokenPriceDto> GetRecordByDatetimeAndPriceTypeAsync(GetTokenPriceByDatetimeInput input,
            PriceType priceType)
        {
            if (priceType == PriceType.FromSwapToken)
            {
                return await _priceAppService.GetSwapTokenPriceByDatetimeAsync(input);
            }

            return await _priceAppService.GetExchangePriceByDatetimeAsync(input);
        }
    }
}