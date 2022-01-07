using System;
using TokenPriceUpdate.Price;

namespace TokenPriceUpdate
{
    public class NewTokenPairCreated
    {
        public PriceType PriceType { get; set; }
        public string Token { get; set; }
        public string UnderlyingToken { get; set; }
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }
    }
}