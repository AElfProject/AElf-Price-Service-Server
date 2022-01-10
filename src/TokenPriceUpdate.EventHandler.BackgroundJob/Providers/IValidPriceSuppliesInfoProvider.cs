using System.Collections.Generic;
using Microsoft.Extensions.Options;
using TokenPriceUpdate.EventHandler.BackgroundJob.Options;
using Volo.Abp.DependencyInjection;

namespace TokenPriceUpdate.EventHandler.BackgroundJob.Providers
{
    public interface IValidPriceSuppliesInfoProvider
    {
        IEnumerable<string> GetValidPriceSuppliers();
        int GetValidPriceSuppliersThresholdCount();
    }
    
    public class DefaultValidPriceSuppliesInfoProvider: IValidPriceSuppliesInfoProvider, ISingletonDependency
    {
        private readonly List<string> _validPriceSuppliers;
        private readonly int _validPriceSuppliersThresholdCount;

        public DefaultValidPriceSuppliesInfoProvider(IOptionsSnapshot<PriceOptions> options)
        {
            _validPriceSuppliers = options.Value.ValidPriceSuppliers;
            _validPriceSuppliersThresholdCount = options.Value.ValidSuppliersCount;
        }

        public IEnumerable<string> GetValidPriceSuppliers()
        {
            return _validPriceSuppliers;
        }

        public int GetValidPriceSuppliersThresholdCount()
        {
            return _validPriceSuppliersThresholdCount;
        }
    }
}