using System.Linq.Expressions;
using CaseConverter;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;

namespace FluentConfiguration.Configurations;

public class ElasticsearchConfigBuilder<T>
    where T : class
{
    public ElasticsearchConfiguration<T> Configuration => configuration;
    private readonly ElasticsearchConfiguration<T> configuration = new();

    public ElasticsearchConfigBuilder<T> HasKey(Expression<Func<T, object>> DocumentId)
    {
        configuration.DocumentId = DocumentId;
        return this;
    }

    public ElasticsearchConfigBuilder<T> Properties(Action<PropertiesDescriptor<T>> configure)
    {
        configuration.Mapping = configure;
        return this;
    }

    public ElasticsearchConfigBuilder<T> Settings(Action<IndexSettingsDescriptor> configure)
    {
        configuration.Settings = configure;
        return this;
    }

    // prefix_domain
    public ElasticsearchConfigBuilder<T> ToIndex(string? prefix = null)
    {
        string domain = typeof(T).Name.ToKebabCase();

        List<string> parts = [];
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            parts.Add(prefix.ToLowerInvariant());
        }

        parts.Add(domain);
        configuration.IndexName = string.Join("_", parts);
        return this;
    }

    public ElasticsearchConfigBuilder<T> Ignores(
        params Expression<Func<T, object>>[] ignoredProperties
    )
    {
        configuration.IgnoredProperties = [.. ignoredProperties];
        return this;
    }
}
