using System;
using EsRepository;
using Nest;
using Volo.Abp.Domain.Entities;

namespace TokenPriceUpdate.Price.Entities.Es
{
    public class PriceRecord : PriceInfoBase, IIndexBuild, IEntity<Guid>
    {
        [Keyword] public Guid Id { get; set; }

        public object[] GetKeys()
        {
            return new object[] {Id};
        }
    }
}