using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TokenPriceUpdate;
using TokenPriceUpdate.Price;
using TokenPriceUpdate.Price.DTOs;
using TokenPriceUpdate.Price.ETOs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;

namespace TokenPrice.EntityHandler.Core
{
    public class TokenPriceUpdateHandler : IDistributedEventHandler<EntityUpdatedEto<PriceEto>>,
        IDistributedEventHandler<EntityCreatedEto<PriceEto>>,
        ITransientDependency
    {
        private readonly IPriceAppService _priceAppService;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly ILogger<TokenPriceUpdateHandler> _logger;

        public TokenPriceUpdateHandler(IPriceAppService priceAppService, IDistributedEventBus distributedEventBus,
            ILogger<TokenPriceUpdateHandler> logger)
        {
            _priceAppService = priceAppService;
            _distributedEventBus = distributedEventBus;
            _logger = logger;
        }

        public async Task HandleEventAsync(EntityUpdatedEto<PriceEto> eventData)
        {
            LogPriceInfo(eventData.Entity, true);
            await HandlePriceEntityAsync(eventData.Entity);
        }

        public async Task HandleEventAsync(EntityCreatedEto<PriceEto> eventData)
        {
            LogPriceInfo(eventData.Entity, false);
            await HandlePriceEntityAsync(eventData.Entity);
        }

        private async Task HandlePriceEntityAsync(PriceEto priceEto)
        {
            await _priceAppService.CreateOrUpdateTokenPriceOnEsAsync(new CreateOrUpdateTokenPriceOnEsInput
            {
                Id = priceEto.Id,
                Token = priceEto.TokenSymbol,
                UnderlyingToken = priceEto.UnderlyingTokenSymbol,
                DateTime = priceEto.DateTime,
                Price = priceEto.Price,
                PriceType = priceEto.PriceType
            });
            await _distributedEventBus.PublishAsync(new NewTokenPairCreated
            {
                PriceType = priceEto.PriceType,
                Token = priceEto.TokenSymbol,
                UnderlyingToken = priceEto.UnderlyingTokenSymbol,
                Price = priceEto.Price
            });
        }

        private void LogPriceInfo(PriceEto priceEto, bool isUpdate)
        {
            var opType = isUpdate ? "Price Update" : "Price Create";
            _logger.LogInformation(
                $"{opType} for token pair: {priceEto.TokenSymbol}-{priceEto.UnderlyingTokenSymbol} price: {priceEto.Price}  priceType: {priceEto.PriceType}  datetime: {priceEto.DateTime}");
        }
    }
}