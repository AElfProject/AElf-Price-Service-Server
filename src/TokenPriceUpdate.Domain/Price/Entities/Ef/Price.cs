using System;
using Volo.Abp.Domain.Entities;

namespace TokenPriceUpdate.Price.Entities.Ef
{
    public class Price : PriceBase, IEntity<Guid>
    {
        public Guid Id { get; set; }

        public object[] GetKeys()
        {
            return new object[] {Id};
        }
    }
}