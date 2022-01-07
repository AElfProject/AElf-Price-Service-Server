using System.Collections.Generic;
using TokenPriceUpdate.Options;
using Volo.Abp.Modularity;

namespace TokenPriceUpdate
{
    [DependsOn(
        typeof(TokenPriceUpdateApplicationModule),
        typeof(TokenPriceUpdateDomainTestModule)
        )]
    public class TokenPriceUpdateApplicationTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<StableCoinOptions>(options =>
            {
                options.StableCoins.Add("ELF");
                options.StableCoins.Add("USDT");
                options.StableCoins.Add("YAN");
                options.StableCoins.Add("RMB");
                options.MaxPathLimit = 2;
            });
        }
    }
}