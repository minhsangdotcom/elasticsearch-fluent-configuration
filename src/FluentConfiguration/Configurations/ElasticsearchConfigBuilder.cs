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

    /// <summary>
    /// Creates the Elasticsearch index name for the entity <typeparamref name="T"/>.
    /// </summary>
    /// <param name="name">
    /// Optional custom index name. If not provided, the entity type name of <typeparamref name="T"/> is used.
    /// </param>
    /// <param name="prefix">
    /// Optional prefix used to distinguish indexes across multiple projects or bounded contexts.
    /// </param>
    /// <param name="case">
    /// Defines how the default index name is formatted. Elasticsearch typically uses kebab-case or snake-case.
    /// Default is <see cref="IndexNameCase.KebabCase"/>.
    /// </param>
    /// <param name="delimiter">
    /// The delimiter applied between the prefix and the index name, default is "_" .
    /// </param>
    /// <returns></returns>
    public ElasticsearchConfigBuilder<T> ToIndex(
        string? name = null,
        string? prefix = null,
        string delimiter = "_",
        IndexNameCase @case = IndexNameCase.KebabCase
    )
    {
        string indexName = string.Empty;
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            indexName = $"{prefix}{delimiter}";
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            indexName += name;
        }
        else
        {
            string defaultName = typeof(T).Name;
            indexName += @case switch
            {
                IndexNameCase.SnakeCase => defaultName.ToSnakeCase(),
                _ => defaultName.ToKebabCase(),
            };
        }

        configuration.IndexName = indexName;
        return this;
    }

    public ElasticsearchConfigBuilder<T> Ignores(List<Expression<Func<T, object>>> ignoreProperties)
    {
        configuration.IgnoreProperties = ignoreProperties;
        return this;
    }
}
