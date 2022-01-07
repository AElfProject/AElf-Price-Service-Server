using System.Threading.Tasks;
using AElf.AElfNode.EventHandler.BackgroundJob;
using AElf.AElfNode.EventHandler.BackgroundJob.EventProcessor;
using AElf.Contracts.Price;

namespace TokenPriceUpdate.EventHandler.BackgroundJob.Processors
{
    public class SyncProcessor : AElfEventProcessorBase<NewestSwapPriceUpdated>
    {
        protected override Task HandleEventAsync(NewestSwapPriceUpdated eventDetailsEto, EventContext txContext)
        {
            return base.HandleEventAsync(eventDetailsEto, txContext);
        }
    }
}