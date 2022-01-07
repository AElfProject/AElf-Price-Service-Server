using Microsoft.Extensions.DependencyInjection;
using TokenPriceUpdate.Options;
using TokenPriceUpdate.Providers;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Threading;

namespace TokenPriceUpdate
{
    [DependsOn(
        typeof(TokenPriceUpdateDomainModule),
        typeof(TokenPriceUpdateApplicationContractsModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpPermissionManagementApplicationModule),
        typeof(AbpFeatureManagementApplicationModule),
        typeof(AbpSettingManagementApplicationModule)
    )]
    public class TokenPriceUpdateApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<StableCoinOptions>(configuration.GetSection("StableCoin"));
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<TokenPriceUpdateApplicationModule>();
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var swapTokeQueryIndexProvider = context.ServiceProvider.GetRequiredService<ISwapTokenPriceQueryIndexProvider>();
            AsyncHelper.RunSync(swapTokeQueryIndexProvider.Initialize);
        }
    }
}
