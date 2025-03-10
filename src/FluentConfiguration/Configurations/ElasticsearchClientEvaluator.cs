using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport.Products.Elasticsearch;

namespace FluentConfiguration.Configurations;

public class ElasticsearchClientEvaluator(ElasticsearchClient elasticsearchClient) : IEvaluator
{
    public async Task Evaluate<TEntity>(ElasticsearchConfigBuilder<TEntity> builder)
        where TEntity : class
    {
        string indexName =
            builder.Configuration.IndexName ?? throw new Exception("Missing index name.");

        Action<PropertiesDescriptor<TEntity>> maps =
            builder.Configuration.Mapping ?? throw new Exception("Missing mapping properties.");

        if (!(await elasticsearchClient.PingAsync()).IsSuccess())
        {
            Console.WriteLine($"cannot connect with elasticsearch server");
            return;
        }

        await CreateIndexOrPutMappingAsync(indexName, maps, builder.Configuration.Settings);
    }

    private async Task CreateIndexOrPutMappingAsync<TEntity>(
        string indexName,
        Action<PropertiesDescriptor<TEntity>> properties,
        Action<IndexSettingsDescriptor>? settings = null
    )
        where TEntity : class
    {
        void CreateIndexDescriptor(CreateIndexRequestDescriptor config)
        {
            CreateIndexRequestDescriptor requestDescriptor = config;

            if (settings != null)
            {
                requestDescriptor = config.Settings(settings);
            }
            requestDescriptor = config.Mappings(typeMap => typeMap.Properties(properties));
        }

        var existsResponse = await elasticsearchClient.Indices.ExistsAsync(indexName);
        ElasticsearchResponse elasticsearchResponse = !existsResponse.Exists
            ? await elasticsearchClient.Indices.CreateAsync(indexName, CreateIndexDescriptor)
            : await elasticsearchClient.Indices.PutMappingAsync(
                indexName,
                config => config.Properties(properties)
            );

        string action = !existsResponse.Exists ? "Create" : "Update";

        if (elasticsearchResponse.IsSuccess())
        {
            Console.WriteLine($@"{action} elasticsearch {indexName} index sucessfully!");
        }
        else
        {
            Console.WriteLine(
                $"{action} elasticsearch {indexName} index mapping has failed with {elasticsearchResponse.DebugInformation}"
            );
        }
    }
}
