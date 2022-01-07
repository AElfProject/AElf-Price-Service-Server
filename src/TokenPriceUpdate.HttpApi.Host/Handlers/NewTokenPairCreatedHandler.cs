using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TokenPriceUpdate.Price;
using TokenPriceUpdate.Price.DTOs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace TokenPriceUpdate.Handlers
{
    public class NewTokenPairCreatedHandler : IDistributedEventHandler<NewTokenPairCreated>, ITransientDependency
    {
        private readonly IPriceAppService _priceAppService;
        private readonly ILogger<NewTokenPairCreatedHandler> _logger;

        public NewTokenPairCreatedHandler(IPriceAppService priceAppService, ILogger<NewTokenPairCreatedHandler> logger)
        {
            _priceAppService = priceAppService;
            _logger = logger;
        }

        public Task HandleEventAsync(NewTokenPairCreated eventData)
        {
            _logger.LogInformation(
                $"Token price changed, source: {eventData.PriceType} token: {eventData.Token}-{eventData.UnderlyingToken} price: {eventData.Price}");
            _priceAppService.UpdateQueryIndexAndCachePrice(new UpdateQueryIndexAndCachePriceInput
            {
                Token = eventData.Token,
                UnderlyingToken = eventData.UnderlyingToken,
                Price = eventData.Price,
                PriceType = eventData.PriceType,
                DateTime = eventData.DateTime
            });
            return Task.CompletedTask;
        }
    }
}