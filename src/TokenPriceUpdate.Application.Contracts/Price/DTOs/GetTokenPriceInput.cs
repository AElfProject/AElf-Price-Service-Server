using System.ComponentModel.DataAnnotations;

namespace TokenPriceUpdate.Price.DTOs
{
    public class GetTokenPriceInput
    {
        public GetTokenPriceInput()
        {
        }

        public GetTokenPriceInput(string token, string underlyingToken)
        {
            Token = token;
            UnderlyingToken = underlyingToken;
        }

        [Required] public string Token { get; set; }
        [Required] public string UnderlyingToken { get; set; }
    }
}