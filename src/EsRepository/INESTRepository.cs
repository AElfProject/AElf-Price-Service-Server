using Volo.Abp.Domain.Entities;

namespace EsRepository
{
    public interface INESTRepository<TEntity, TKey> : INESTReaderRepository<TEntity, TKey>,
        INESTWriterRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
    }
}