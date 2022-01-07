using System;
using System.Collections.Generic;

namespace TokenPriceUpdate.Price.DTOs
{
    public class TokenPriceRecordDto
    {
        public string Token { get; set; }
        public string UnderlyingToken { get; set; }
        public long TotalCount { get; set; }
        public List<PriceInfoDto> PriceInfoList { get; set; } = new();

    }

    public class PriceInfoDto
    {
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }
    }
}