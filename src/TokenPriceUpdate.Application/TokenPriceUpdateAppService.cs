using TokenPriceUpdate.Localization;
using Volo.Abp.Application.Services;

namespace TokenPriceUpdate
{
    /* Inherit your application services from this class.
     */
    public abstract class TokenPriceUpdateAppService : ApplicationService
    {
        protected TokenPriceUpdateAppService()
        {
            LocalizationResource = typeof(TokenPriceUpdateResource);
        }
    }
}
