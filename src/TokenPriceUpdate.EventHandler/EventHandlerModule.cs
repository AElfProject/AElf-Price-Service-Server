using AElf.AElfNode.EventHandler;
using AElf.AElfNode.EventHandler.BackgroundJob.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokenPriceUpdate.EntityFrameworkCore;
using TokenPriceUpdate.EventHandler.BackgroundJob;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.EventBus.RabbitMq;

namespace TokenPriceUpdate.EventHandler
{

    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(TokenPriceUpdateEntityFrameworkCoreModule),
        typeof(TokenPriceUpdateEventHandlerBackgroundJobModule),
        typeof(AElfNodeEventHandlerModule),
        typeof(AbpEventBusRabbitMqModule)
    )]
    public class EventHandlerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostEnvironment = context.Services.GetSingletonInstance<IHostEnvironment>();
            Configure<AElfProcessorOption>(options =>
            {
                configuration.GetSection("EventProcessors").Bind(options);
            });

            context.Services.AddHostedService<EventHandlerHostedService>();
        }
    }
}
