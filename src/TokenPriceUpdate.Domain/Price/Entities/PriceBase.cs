using System;
using Nest;

namespace TokenPriceUpdate.Price.Entities
{
    public class PriceBase
    {
        [Keyword] public string TokenSymbol { get; set; }
        [Keyword] public string UnderlyingTokenSymbol { get; set; }
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }
        public PriceType PriceType { get; set; }
    }
}