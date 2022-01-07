using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace TokenPriceUpdate
{
    [Dependency(ReplaceServices = true)]
    public class TokenPriceUpdateBrandingProvider : DefaultBrandingProvider
    {
        public override string AppName => "TokenPriceUpdate";
    }
}
