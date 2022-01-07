using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace EsRepository
{
    public interface INESTWriterRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        Task AddOrUpdateAsync(TEntity model);
        
        Task AddAsync(TEntity model);
        
        Task UpdateAsync(TEntity model);

        Task DeleteAsync(TEntity model);
    }
}