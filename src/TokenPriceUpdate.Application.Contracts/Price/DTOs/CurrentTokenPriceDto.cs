namespace TokenPriceUpdate.Price.DTOs
{
    public class CurrentTokenPriceDto
    {
        public string Token { get; set; }
        public string UnderlyingToken { get; set; }
        public decimal Price { get; set; }
    }
}