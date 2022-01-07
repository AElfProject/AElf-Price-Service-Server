using System.Threading.Tasks;

namespace TokenPriceUpdate.Data
{
    public interface ITokenPriceUpdateDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}