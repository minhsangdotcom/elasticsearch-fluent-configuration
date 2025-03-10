namespace FluentConfiguration.Configurations;

public interface IEvaluator
{
    Task Evaluate<TEntity>(ElasticsearchConfigBuilder<TEntity> builder)
        where TEntity : class;
}

public interface IEvaluatorSync
{
    void Evaluate<TEntity>(ElasticsearchConfigBuilder<TEntity> builder)
        where TEntity : class;
}
