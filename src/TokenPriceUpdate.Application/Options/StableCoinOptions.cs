using System.Collections.Generic;

namespace TokenPriceUpdate.Options
{
    public class StableCoinOptions
    {
        public List<string> StableCoins { get; } = new();
        public int MaxPathLimit = 2;
    }
}