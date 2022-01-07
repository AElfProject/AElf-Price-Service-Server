using EsRepository.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokenPrice.EntityHandler.Core;
using TokenPriceUpdate;
using TokenPriceUpdate.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace TokenPrice.EntityHandler
{

    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(TokenPriceEntityHandlerCoreModule),
        typeof(TokenPriceUpdateEntityFrameworkCoreModule),
        typeof(AbpEventBusRabbitMqModule)
    )]
    public class EntityHandlerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostEnvironment = context.Services.GetSingletonInstance<IHostEnvironment>();
            ConfigureEsIndexCreation();
            context.Services.AddHostedService<EntityHandlerHostedService>();
        }
        
        private void ConfigureEsIndexCreation()
        {
            Configure<IndexCreateOption>(x =>
            {
                x.AddModule(typeof(TokenPriceUpdateDomainModule));
            });
        }
    }
}
