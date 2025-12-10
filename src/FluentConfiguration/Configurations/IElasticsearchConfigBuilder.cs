namespace FluentConfiguration.Configurations;

public interface IElasticsearchDocumentConfigure<T>
    where T : class
{
    void Configure(
        ref ElasticsearchConfigBuilder<T> builder,
        string? prefix = null,
        string? delimiter = null
    );
}
