using Nest;

namespace TokenPriceUpdate.Price.Entities.Es
{
    public class PriceInfoBase: PriceBase
    {
        [Keyword] public string TokenKey { get; set; }
    }
}