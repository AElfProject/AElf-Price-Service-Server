using System;

namespace TokenPriceUpdate.Price.DTOs
{
    public class UpdateQueryIndexAndCachePriceInput
    {
        public PriceType PriceType { get; set; }
        public string Token { get; set; }
        public string UnderlyingToken { get; set; }
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }
    }
}