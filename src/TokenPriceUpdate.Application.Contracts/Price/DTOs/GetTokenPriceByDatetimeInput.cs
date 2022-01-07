namespace TokenPriceUpdate.Price.DTOs
{
    public class GetTokenPriceByDatetimeInput: GetTokenPriceInput
    {
        public GetTokenPriceByDatetimeInput()
        {
            
        }

        public GetTokenPriceByDatetimeInput(string token, string underlyingToken, long time): base(token, underlyingToken)
        {
            Time = time;
        }
        public long Time { get; set; }
    }
}