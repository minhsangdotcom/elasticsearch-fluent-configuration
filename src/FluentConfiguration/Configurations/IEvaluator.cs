namespace FluentConfiguration.Configurations;

public interface IAsyncEvaluator
{
    Task EvaluateAsync<TEntity>(ElasticsearchConfigBuilder<TEntity> builder)
        where TEntity : class;
}

public interface IEvaluator
{
    void Evaluate<TEntity>(ElasticsearchConfigBuilder<TEntity> builder)
        where TEntity : class;
}
