using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TokenPriceUpdate.Data;
using Volo.Abp.DependencyInjection;

namespace TokenPriceUpdate.EntityFrameworkCore
{
    public class EntityFrameworkCoreTokenPriceUpdateDbSchemaMigrator
        : ITokenPriceUpdateDbSchemaMigrator, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityFrameworkCoreTokenPriceUpdateDbSchemaMigrator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task MigrateAsync()
        {
            /* We intentionally resolving the TokenPriceUpdateDbContext
             * from IServiceProvider (instead of directly injecting it)
             * to properly get the connection string of the current tenant in the
             * current scope.
             */

            await _serviceProvider
                .GetRequiredService<TokenPriceUpdateDbContext>()
                .Database
                .MigrateAsync();
        }
    }
}
