using System;
using Elasticsearch.Net;
using EsRepository.Options;
using Microsoft.Extensions.Options;
using Nest;
using Volo.Abp.DependencyInjection;

namespace EsRepository.Provider
{
    public interface IEsClientProvider
    {
        ElasticClient GetClient();
    }
    
    public class DefaultEsClientProvider: IEsClientProvider, ISingletonDependency
    {
        private readonly Lazy<ElasticClient> _client;
        
        public DefaultEsClientProvider(IOptions<EsEndpointOption> uriOptions)
        {
            var uris = uriOptions.Value.Uris.ConvertAll(x => new Uri(x));
            var connectionPool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(connectionPool);
            _client = new Lazy<ElasticClient>(() => new ElasticClient(settings));
        }
        public ElasticClient GetClient()
        {
            return _client.Value;
        }
    }
}