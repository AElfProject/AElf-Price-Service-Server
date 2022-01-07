using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TokenPriceUpdate.Data
{
    /* This is used if database provider does't define
     * ITokenPriceUpdateDbSchemaMigrator implementation.
     */
    public class NullTokenPriceUpdateDbSchemaMigrator : ITokenPriceUpdateDbSchemaMigrator, ITransientDependency
    {
        public Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }
}