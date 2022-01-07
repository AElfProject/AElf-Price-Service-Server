using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elasticsearch.Net;
using EsRepository;
using EsRepository.Options;
using EsRepository.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TokenPriceUpdate.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace TokenPriceUpdate
{
    [DependsOn(
        typeof(TokenPriceUpdateEntityFrameworkCoreTestModule)
        )]
    public class TokenPriceUpdateDomainTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<IndexCreateOption>(x =>
            {
                x.AddModule(typeof(TokenPriceUpdateDomainModule));
            });
            // Do not modify this!!!
            context.Services.Configure<EsEndpointOption>(options =>
            {
                options.Uris = new List<string> { "http://127.0.0.1:9200"};
            });
            
            context.Services.Configure<IndexSettingOptions>(options =>
            {
                options.NumberOfReplicas = 1;
                options.NumberOfShards = 1;
                options.Refresh = Refresh.True;
            });
        }
        
        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            var elasticIndexService = context.ServiceProvider.GetRequiredService<IElasticIndexService>();
            var modules = context.ServiceProvider.GetRequiredService<IOptions<IndexCreateOption>>().Value.Modules;
            
            modules.ForEach(m =>
            {
                var types = GetTypesAssignableFrom<IIndexBuild>(m.Assembly);
                foreach (var t in types)
                {
                    AsyncHelper.RunSync(async () =>
                        await elasticIndexService.DeleteIndexAsync(t.Name.ToLower()));
                }
            });
        }

        private List<Type> GetTypesAssignableFrom<T>(Assembly assembly)
        {
            var compareType = typeof(T);
            return assembly.DefinedTypes
                .Where(type => compareType.IsAssignableFrom(type) && !compareType.IsAssignableFrom(type.BaseType) &&
                               !type.IsAbstract && type.IsClass && compareType != type)
                .Cast<Type>().ToList();
        }

    }
}