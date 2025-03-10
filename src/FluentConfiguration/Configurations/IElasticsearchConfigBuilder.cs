namespace FluentConfiguration.Configurations;

public interface IElasticsearchDocumentConfigure<T>
    where T : class
{
    void Configure(ref ElasticsearchConfigBuilder<T> buider, string? prefix = null);
}
