using System.Collections.Generic;
using System.Linq;
using TokenPriceUpdate.EventHandler.BackgroundJob.Providers;
using Volo.Abp.DependencyInjection;

namespace TokenPriceUpdate.EventHandler.BackgroundJob.Services
{
    public interface IValidPriceSupplierService
    {
        bool ValidPriceSuppliers(IEnumerable<string> suppliers);
    }
    
    public class ValidPriceSupplierService: IValidPriceSupplierService, ITransientDependency
    {
        private readonly IValidPriceSuppliesInfoProvider _validPriceSuppliesInfoProvider;

        public ValidPriceSupplierService(IValidPriceSuppliesInfoProvider validPriceSuppliesInfoProvider)
        {
            _validPriceSuppliesInfoProvider = validPriceSuppliesInfoProvider;
        }

        public bool ValidPriceSuppliers(IEnumerable<string> suppliers)
        {
            var threshold = _validPriceSuppliesInfoProvider.GetValidPriceSuppliersThresholdCount();
            var validPriceSuppliers = _validPriceSuppliesInfoProvider.GetValidPriceSuppliers();
            return suppliers.Count(x => validPriceSuppliers.Contains(x)) >= threshold;
        }
    }
}