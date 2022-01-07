using TokenPriceUpdate.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace TokenPriceUpdate.DbMigrator
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(TokenPriceUpdateEntityFrameworkCoreModule),
        typeof(TokenPriceUpdateApplicationContractsModule)
    )]
    public class TokenPriceUpdateDbMigratorModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options =>
            {
                options.IsJobExecutionEnabled = false;
            });
        }
    }
}
