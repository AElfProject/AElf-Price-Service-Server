using System;

namespace TokenPriceUpdate.Price.DTOs
{
    public class TokenPriceDto: CurrentTokenPriceDto
    {
        public TokenPriceDto()
        {
        }

        public TokenPriceDto(string token, string underlyingToken)
        {
            Token = token;
            UnderlyingToken = underlyingToken;
        }
        public DateTime DateTime { get; set; }
    }
}