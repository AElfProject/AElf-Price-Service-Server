using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TokenPriceUpdate.EntityFrameworkCore
{
    /* This class is needed for EF Core console commands
     * (like Add-Migration and Update-Database commands) */
    public class TokenPriceUpdateDbContextFactory : IDesignTimeDbContextFactory<TokenPriceUpdateDbContext>
    {
        public TokenPriceUpdateDbContext CreateDbContext(string[] args)
        {
            TokenPriceUpdateEfCoreEntityExtensionMappings.Configure();

            var configuration = BuildConfiguration();
            
            var builder = new DbContextOptionsBuilder<TokenPriceUpdateDbContext>()
                .UseMySql(configuration.GetConnectionString("Default"), MySqlServerVersion.LatestSupportedServerVersion);
            
            return new TokenPriceUpdateDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TokenPriceUpdate.DbMigrator/"))
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
