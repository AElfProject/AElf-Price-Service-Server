using System;
using EsRepository;
using Nest;
using Volo.Abp.Domain.Entities;

namespace TokenPriceUpdate.Price.Entities.Es
{
    public class Price : PriceInfoBase, IIndexBuild, IEntity<Guid>
    {
        [Keyword] public Guid Id { get; set; }

        public object[] GetKeys()
        {
            return new object[] {Id};
        }
    }
}