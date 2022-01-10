using AElf.AElfNode.EventHandler.BackgroundJob;
using Microsoft.Extensions.DependencyInjection;
using TokenPriceUpdate.EventHandler.BackgroundJob.Options;
using TokenPriceUpdate.Price.ETOs;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Modularity;

namespace TokenPriceUpdate.EventHandler.BackgroundJob
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AElfEventHandlerBackgroundJobModule),
        typeof(TokenPriceUpdateApplicationModule))]
    public class TokenPriceUpdateEventHandlerBackgroundJobModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<AbpDistributedEntityEventOptions>(options =>
            {
                options.AutoEventSelectors.Add<Price.Entities.Ef.Price>();
                options.EtoMappings.Add<Price.Entities.Ef.Price, PriceEto>();
            });

            Configure<AbpAutoMapperOptions>(options =>
            {
                //Add all mappings defined in the assembly of the MyModule class
                options.AddMaps<TokenPriceUpdateEventHandlerBackgroundJobModule>();
            });

            Configure<PriceOptions>(configuration.GetSection("PriceOption"));
        }
    }
}