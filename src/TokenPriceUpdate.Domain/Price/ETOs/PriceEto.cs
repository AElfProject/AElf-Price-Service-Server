using Volo.Abp.EventBus;

namespace TokenPriceUpdate.Price.ETOs
{
    [EventName("Price.PriceUpdate")]
    public class PriceEto: Entities.Ef.Price
    {
    }
}