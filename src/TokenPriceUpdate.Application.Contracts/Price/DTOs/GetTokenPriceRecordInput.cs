namespace TokenPriceUpdate.Price.DTOs
{
    public class GetTokenPriceRecordInput : GetTokenPriceInput
    {
        public GetTokenPriceRecordInput()
        {
        }

        public GetTokenPriceRecordInput(string token, string underlyingToken) : base(token, underlyingToken)
        {
        }

        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public int SkipCount { get; set; }
        public int Size { get; set; }
    }
}