using System.Collections.Generic;

namespace TokenPriceUpdate.EventHandler.BackgroundJob.Options
{
    public class PriceOptions
    {
        public List<string> ValidPriceSuppliers { get; set; } = new();
        public int ValidSuppliersCount { get; set; }
    }
}