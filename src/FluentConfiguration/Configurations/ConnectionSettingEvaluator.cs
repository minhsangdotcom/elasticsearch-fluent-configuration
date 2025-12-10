using Elastic.Clients.Elasticsearch;

namespace FluentConfiguration.Configurations;

public class ConnectionSettingEvaluator(ElasticsearchClientSettings settings) : IEvaluator
{
    public void Evaluate<TEntity>(ElasticsearchConfigBuilder<TEntity> builder)
        where TEntity : class
    {
        void Selector(ClrTypeMappingDescriptor<TEntity> descriptor)
        {
            if (builder.Configuration.DocumentId != null)
            {
                descriptor.IdProperty(builder.Configuration.DocumentId);
            }
        }

        settings.DefaultMappingFor((Action<ClrTypeMappingDescriptor<TEntity>>)Selector);
    }
}
