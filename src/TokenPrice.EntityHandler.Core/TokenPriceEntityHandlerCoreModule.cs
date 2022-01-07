using TokenPriceUpdate;
using Volo.Abp.Modularity;

namespace TokenPrice.EntityHandler.Core
{
    [DependsOn(
        typeof(TokenPriceUpdateApplicationModule)
    )]
    public class TokenPriceEntityHandlerCoreModule: AbpModule
    {
    }
}


