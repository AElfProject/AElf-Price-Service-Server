using System.Threading.Tasks;
using AElf.AElfNode.EventHandler.BackgroundJob;
using AElf.AElfNode.EventHandler.BackgroundJob.EventProcessor;
using AElf.Contracts.Price;
using TokenPriceUpdate.Price;
using TokenPriceUpdate.Price.DTOs;

namespace TokenPriceUpdate.EventHandler.BackgroundJob.Processors
{
    public class NewestSwapPriceUpdatedProcessor : AElfEventProcessorBase<NewestSwapPriceUpdated>
    {
        private readonly IPriceAppService _priceAppService;

        public NewestSwapPriceUpdatedProcessor(IPriceAppService priceAppService)
        {
            _priceAppService = priceAppService;
        }

        protected override async Task HandleEventAsync(NewestSwapPriceUpdated eventDetailsEto, EventContext txContext)
        {
            if (txContext.Status != BackgroundJobConstants.MindStatus)
            {
                return;
            }

            await _priceAppService.CreateOrUpdateTokenPriceAsync(new CreateOrUpdateTokenPriceInput
            {
                Token = eventDetailsEto.TokenSymbol,
                UnderlyingToken = eventDetailsEto.TargetTokenSymbol,
                Price = decimal.Parse(eventDetailsEto.Price),
                DateTime = eventDetailsEto.Timestamp.ToDateTime(),
                PriceType = PriceType.FromSwapToken
            });
        }
    }
}