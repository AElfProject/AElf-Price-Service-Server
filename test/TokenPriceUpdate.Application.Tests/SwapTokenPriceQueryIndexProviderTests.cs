using Shouldly;
using TokenPriceUpdate.Providers;
using Xunit;

namespace TokenPriceUpdate
{
    public class SwapTokenPriceQueryIndexProviderTests: TokenPriceUpdateTestBase<TokenPriceUpdateApplicationTestModule>
    {
        private readonly ISwapTokenPriceQueryIndexProvider _tokenPriceQueryIndexProvider;
        private readonly string[] _underlyingBaseTokens;

        public SwapTokenPriceQueryIndexProviderTests()
        {
            _tokenPriceQueryIndexProvider = GetRequiredService<ISwapTokenPriceQueryIndexProvider>();
            _underlyingBaseTokens = TokenNode.RootTokenNodeNames;
        }
        
        [Fact]
        public void GetTokenPriceIndex_Underlying_Token_Should_Be_Initialized()
        {
            foreach (var underlyingToken in _underlyingBaseTokens)
            {
                var tokenNode = _tokenPriceQueryIndexProvider.GetTokenNode(underlyingToken);
                tokenNode.ShouldNotBeNull();
                tokenNode.Neighbours.Count.ShouldBe(0);
                foreach (var targetNodeInfoKp in tokenNode.TargetNodeInfo)
                {
                    if (targetNodeInfoKp.Key == underlyingToken)
                    {
                        targetNodeInfoKp.Value.Item2.ShouldBe(0);
                        continue;
                    }
                    
                    targetNodeInfoKp.Value.Item1.ShouldBe(string.Empty);
                    targetNodeInfoKp.Value.Item2.ShouldBe(TokenNode.InfinitePathWeight);
                }
            }
        }

        [Fact]
        public void UpdateTokenPriceIndex_Add_Underlying_Swap_Should_Get_Right_Query_Index()
        {
            foreach (var underlyingToken in _underlyingBaseTokens)
            {
                var tokenSymbol = "XIN";
                _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(tokenSymbol, underlyingToken);
                var queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(tokenSymbol, underlyingToken);
                queryTokenIndex.Count.ShouldBe(1);
                queryTokenIndex[0].Item1.ShouldBe(tokenSymbol);
                queryTokenIndex[0].Item2.ShouldBe(underlyingToken);
            }
        }
        
        [Fact]
        public void UpdateTokenPriceIndex_Add_Swap_Should_Get_Right_Query_Index()
        {
            var underlyingToken = _underlyingBaseTokens[0];
            var tokenSymbolOne = "XIN";
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(tokenSymbolOne, underlyingToken);
            var tokenSymbolTwo = "JIN";
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(tokenSymbolTwo, tokenSymbolOne);
            var queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(tokenSymbolTwo, underlyingToken);
            queryTokenIndex.Count.ShouldBe(2);
            queryTokenIndex[0].Item1.ShouldBe(tokenSymbolTwo);
            queryTokenIndex[0].Item2.ShouldBe(tokenSymbolOne);
            queryTokenIndex[1].Item1.ShouldBe(tokenSymbolOne);
            queryTokenIndex[1].Item2.ShouldBe(underlyingToken);
        }
        
        [Fact]
        public void UpdateTokenPriceIndex_Add_Swap_Query_Without_Underlying_Token_Should_Get_Right_Query_Index()
        {
            var underlyingToken = _underlyingBaseTokens[0];
            var tokenSymbolOne = "XIN";
            var tokenSymbolTwo = "JIN";
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(tokenSymbolOne, underlyingToken);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(tokenSymbolTwo, tokenSymbolOne);
            var queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(tokenSymbolTwo, tokenSymbolOne);
            queryTokenIndex.Count.ShouldBe(1);
            queryTokenIndex[0].Item1.ShouldBe(tokenSymbolTwo);
            queryTokenIndex[0].Item2.ShouldBe(tokenSymbolOne);
            
            queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(tokenSymbolOne, tokenSymbolTwo);
            queryTokenIndex.Count.ShouldBe(1);
            queryTokenIndex[0].Item1.ShouldBe(tokenSymbolOne);
            queryTokenIndex[0].Item2.ShouldBe(tokenSymbolTwo);
        }
        
        [Fact]
        public void UpdateTokenPriceIndex_Add_Multiple_Swap_Should_Get_Right_Query_Index()
        {
            var underlyingTokenOne = _underlyingBaseTokens[0];
            var underlyingTokenTwo = _underlyingBaseTokens[1];
            var underlyingTokenThree = _underlyingBaseTokens[2];
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(underlyingTokenOne, underlyingTokenTwo);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(underlyingTokenThree, underlyingTokenTwo);
            var queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(underlyingTokenTwo, underlyingTokenOne);
            queryTokenIndex.Count.ShouldBe(1);
            queryTokenIndex[0].Item1.ShouldBe(underlyingTokenTwo);
            queryTokenIndex[0].Item2.ShouldBe(underlyingTokenOne);
            
            queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(underlyingTokenOne, underlyingTokenTwo);
            queryTokenIndex.Count.ShouldBe(1);
            queryTokenIndex[0].Item1.ShouldBe(underlyingTokenOne);
            queryTokenIndex[0].Item2.ShouldBe(underlyingTokenTwo);
            
            queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(underlyingTokenOne, underlyingTokenThree);
            queryTokenIndex.Count.ShouldBe(2);
            queryTokenIndex[0].Item1.ShouldBe(underlyingTokenOne);
            queryTokenIndex[0].Item2.ShouldBe(underlyingTokenTwo);
            queryTokenIndex[1].Item1.ShouldBe(underlyingTokenTwo);
            queryTokenIndex[1].Item2.ShouldBe(underlyingTokenThree);
        }
        
        // JJ
        // | 
        // v   p1
        // GM  —— > LLPY
        // | p2      | p3
        // v    p4   v
        // WJC —— > ZX
        // | p5      | p6 
        // v    p7   v
        // ELF —— > USDT
        [Fact]
        public void UpdateTokenPriceIndex_Add_Underlying_Token_Swap_Should_Get_Right_Query_Index()
        {
            var token1 = "GM";
            var token2 = "LLYP";
            var token3 = "ZX";
            var token4 = "WJC";
            var token5 = _underlyingBaseTokens[0];
            var token6 = _underlyingBaseTokens[1];
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token1, token2);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token1, token4);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token2, token3);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token4, token3);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token5, token6);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token4, token5);
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token3, token6);

            var queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(token1, token6);
            queryTokenIndex.Count.ShouldBe(0);
            
            queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(token2, token6);
            queryTokenIndex.Count.ShouldBe(2);
            queryTokenIndex[0].Item1.ShouldBe(token2);
            queryTokenIndex[0].Item2.ShouldBe(token3);
            queryTokenIndex[1].Item1.ShouldBe(token3);
            queryTokenIndex[1].Item2.ShouldBe(token6);
            
            queryTokenIndex = _tokenPriceQueryIndexProvider.GetTokenPriceIndex(token4, token6);
            queryTokenIndex.Count.ShouldBe(2);
            queryTokenIndex[0].Item1.ShouldBe(token4);
            queryTokenIndex[0].Item2.ShouldBe(token5);
            queryTokenIndex[1].Item1.ShouldBe(token5);
            queryTokenIndex[1].Item2.ShouldBe(token6);

            var overMaxPathTokenSymbol = "JJ";
            _tokenPriceQueryIndexProvider.UpdateTokenPriceIndex(token1, overMaxPathTokenSymbol);
            var overMaxPathToken = _tokenPriceQueryIndexProvider.GetTokenNode(overMaxPathTokenSymbol);
            overMaxPathToken.Neighbours.ShouldContainKey(token1);
            overMaxPathToken.TargetNodeInfo[token5].Item2.ShouldBe(3);
            overMaxPathToken.TargetNodeInfo[token5].Item1.ShouldBe(token1);
            overMaxPathToken.TargetNodeInfo[token6].Item2.ShouldBe(TokenNode.InfinitePathWeight);
        }
    }
}