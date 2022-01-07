using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EsRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using TokenPriceUpdate.Options;
using TokenPriceUpdate.Price;
using Volo.Abp.DependencyInjection;

namespace TokenPriceUpdate.Providers
{
    public interface ISwapTokenPriceQueryIndexProvider
    {
        Task Initialize();
        List<(string, string)> GetTokenPriceIndex(string token, string underlyingToken);
        void UpdateTokenPriceIndex(string token, string underlyingToken);
        TokenNode GetTokenNode(string token);
    }

    public class DefaultSwapTokenPriceQueryIndexProvider : ISwapTokenPriceQueryIndexProvider, ISingletonDependency
    {
        private readonly Dictionary<string, TokenNode> _tokenNodesDic;
        private readonly INESTReaderRepository<Price.Entities.Es.Price, Guid> _priceRepository;
        private readonly ILogger<DefaultSwapTokenPriceQueryIndexProvider> _logger;
        private readonly int _maxPathLimit;

        public DefaultSwapTokenPriceQueryIndexProvider(IOptionsSnapshot<StableCoinOptions> option,
            ILogger<DefaultSwapTokenPriceQueryIndexProvider> logger,
            INESTReaderRepository<Price.Entities.Es.Price, Guid> priceRepository)
        {
            _logger = logger;
            _priceRepository = priceRepository;
            _maxPathLimit = option.Value.MaxPathLimit;
            _tokenNodesDic = new Dictionary<string, TokenNode>();
            TokenNode.RootTokenNodeNames = option.Value.StableCoins.ToArray();
        }

        public async Task Initialize()
        {
            if (!TokenNode.RootTokenNodeNames.Any())
            {
                return;
            }

            InitializeRootToken();
            var tokenPricesInfo =
                (await _priceRepository.GetListAsync(GetPriceFilterQueryContainer(PriceType.FromSwapToken))).Item2;
            tokenPricesInfo.ForEach(x => UpdateTokenPriceIndex(x.TokenSymbol, x.UnderlyingTokenSymbol));
        }

        private Func<QueryContainerDescriptor<Price.Entities.Es.Price>, QueryContainer> GetPriceFilterQueryContainer(
            PriceType priceType)
        {
            return q =>
            {
                return q
                    .Term(t => t
                        .Field(f => f.PriceType).Value(priceType));
            };
        }

        public List<(string, string)> GetTokenPriceIndex(string token, string underlyingToken)
        {
            var tokenQueryList = new List<(string, string)>();
            if (token == underlyingToken)
            {
                _logger.LogWarning($"Invalid input , same token symbol: {token}");
                return tokenQueryList;
            }

            if (!_tokenNodesDic.TryGetValue(token, out var tokenNode))
            {
                _logger.LogWarning($"Invalid token :{token}");
                return tokenQueryList;
            }

            if (!TokenNode.RootTokenNodeNames.Contains(underlyingToken))
            {
                if (!tokenNode.Neighbours.ContainsKey(underlyingToken))
                {
                    _logger.LogWarning($"Invalid token pair:{token}-{underlyingToken}");
                    return tokenQueryList;
                }

                tokenQueryList.Add((token, underlyingToken));
                return tokenQueryList;
            }

            var (nextToken, path) = tokenNode.TargetNodeInfo[underlyingToken];
            if (path > _maxPathLimit)
            {
                _logger.LogWarning(
                    $"PathWeight limit exceed, pair: {token}-{underlyingToken} PathWeight: {path}  Max PathWeight:{_maxPathLimit}");
                return tokenQueryList;
            }

            if (path == 0)
            {
                return tokenQueryList;
            }

            tokenQueryList.Add((token, nextToken));
            tokenQueryList.AddRange(GetTokenPriceIndex(nextToken, underlyingToken));
            return tokenQueryList;
        }

        public void UpdateTokenPriceIndex(string originalToken, string underlyingToken)
        {
            var originalTokenNode = EnsureNodeExist(originalToken);
            var underlyingTokenNode = EnsureNodeExist(underlyingToken);
            originalTokenNode.AddNeighbour(underlyingTokenNode);
        }

        public TokenNode GetTokenNode(string token)
        {
            return _tokenNodesDic.TryGetValue(token, out var tokenNode) ? tokenNode : null;
        }

        private void InitializeRootToken()
        {
            foreach (var stableCoin in TokenNode.RootTokenNodeNames)
            {
                _tokenNodesDic.TryAdd(stableCoin, new TokenNode(stableCoin, true));
            }
        }

        private TokenNode EnsureNodeExist(string token)
        {
            if (_tokenNodesDic.TryGetValue(token, out var tokenNode))
            {
                return tokenNode;
            }

            tokenNode = new TokenNode(token, true);
            _tokenNodesDic.TryAdd(token, tokenNode);
            return tokenNode;
        }
    }
}