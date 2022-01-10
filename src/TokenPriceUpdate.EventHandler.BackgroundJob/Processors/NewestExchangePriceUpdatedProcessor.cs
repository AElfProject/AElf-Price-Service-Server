using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AElf.AElfNode.EventHandler.BackgroundJob;
using AElf.AElfNode.EventHandler.BackgroundJob.EventProcessor;
using AElf.Contracts.Price;
using Microsoft.Extensions.Logging;
using TokenPriceUpdate.EventHandler.BackgroundJob.Services;
using TokenPriceUpdate.Price;
using TokenPriceUpdate.Price.DTOs;

namespace TokenPriceUpdate.EventHandler.BackgroundJob.Processors
{
    public class NewestExchangePriceUpdatedProcessor : AElfEventProcessorBase<NewestExchangePriceUpdated>
    {
        private readonly IPriceAppService _priceAppService;
        private readonly IValidPriceSupplierService _validPriceSupplierService;
        private readonly ILogger<NewestExchangePriceUpdatedProcessor> _logger;

        public NewestExchangePriceUpdatedProcessor(IPriceAppService priceAppService,
            IValidPriceSupplierService validPriceSupplierService, ILogger<NewestExchangePriceUpdatedProcessor> logger)
        {
            _priceAppService = priceAppService;
            _validPriceSupplierService = validPriceSupplierService;
            _logger = logger;
        }

        protected override async Task HandleEventAsync(NewestExchangePriceUpdated eventDetailsEto,
            EventContext txContext)
        {
            if (txContext.Status != BackgroundJobConstants.MindStatus)
            {
                return;
            }

            var supplierNodeList = eventDetailsEto.PriceSupplier.NodeList.Select(x => x.ToBase58()).ToArray();
            var isValid = _validPriceSupplierService.ValidPriceSuppliers(supplierNodeList);
            if (!isValid)
            {
                _logger.LogWarning("Invalid price suppliers: ");
                var warningMsg = new StringBuilder();
                foreach (var supplierNode in supplierNodeList)
                {
                    warningMsg.Append($"{supplierNode}; ");
                }

                _logger.LogWarning(warningMsg.ToString());
            }

            await _priceAppService.CreateOrUpdateTokenPriceAsync(new CreateOrUpdateTokenPriceInput
            {
                Token = eventDetailsEto.TokenSymbol,
                UnderlyingToken = eventDetailsEto.TargetTokenSymbol,
                Price = decimal.Parse(eventDetailsEto.Price),
                DateTime = eventDetailsEto.Timestamp.ToDateTime(),
                PriceType = PriceType.FromExchange
            });
        }
    }
}