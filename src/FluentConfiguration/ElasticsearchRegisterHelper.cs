using System.Reflection;
using Elastic.Clients.Elasticsearch;
using FluentConfiguration.Configurations;

namespace FluentConfiguration;

public static class ElasticsearchRegisterHelper
{
    /// <summary>
    /// Execute connection mapping config including id, ignore mapping ....
    /// </summary>
    /// <param name="connectionSettings"></param>
    /// <param name="elsConfigs"></param>
    public static void ConfigureConnectionSettings(
        ref ElasticsearchClientSettings connectionSettings,
        IEnumerable<ElasticConfigureResult> configures
    )
    {
        foreach (var configure in configures)
        {
            object? connectionSettingEvaluator = Activator.CreateInstance(
                typeof(ConnectionSettingEvaluator),
                [connectionSettings]
            );

            MethodInfo evaluateMethodInfo = typeof(ConnectionSettingEvaluator)
                .GetMethod(nameof(IEvaluator.Evaluate))!
                .MakeGenericMethod(configure.Type);

            evaluateMethodInfo.Invoke(connectionSettingEvaluator, [configure.Configs]);
        }
    }

    /// <summary>
    /// execute entity configuration
    /// </summary>
    /// <param name="elasticClient"></param>
    /// <param name="elsConfigs"></param>
    /// <returns></returns>
    public static async Task ElasticFluentConfigAsync(
        this ElasticsearchClient elasticClient,
        IEnumerable<ElasticConfigureResult> configures
    )
    {
        foreach (ElasticConfigureResult configure in configures)
        {
            object? elasticsearchClientEvaluator = Activator.CreateInstance(
                typeof(ElasticsearchClientEvaluator),
                [elasticClient]
            );

            MethodInfo evaluateMethodInfo = typeof(ElasticsearchClientEvaluator)
                .GetMethod(nameof(IAsyncEvaluator.EvaluateAsync))!
                .MakeGenericMethod(configure.Type);

            Task evaluateAsync = (Task)
                evaluateMethodInfo.Invoke(elasticsearchClientEvaluator, [configure.Configs])!;
            await evaluateAsync;
        }
    }

    /// <summary>
    /// get all of config classes by reflection
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IEnumerable<ElasticConfigureResult> GetElasticsearchConfigBuilder(
        Assembly assembly,
        string? prefix = null
    )
    {
        List<(Type type, Type iType)> configuringTypes = GetConfiguringTypes(assembly);
        foreach (var (type, iType) in configuringTypes)
        {
            MethodInfo? method = GetConfigureMethod(type);
            if (method == null)
            {
                continue;
            }

            object? elasticsearchConfigBuilder = CreateElasticsearchConfigBuilder(iType);
            object? elsConfig = Activator.CreateInstance(type);

            method.Invoke(elsConfig, [elasticsearchConfigBuilder, prefix]);

            yield return new ElasticConfigureResult(elasticsearchConfigBuilder!, iType);
        }
    }

    private static List<(Type type, Type iType)> GetConfiguringTypes(Assembly assembly)
    {
        return
        [
            .. assembly
                .GetTypes()
                .Where(type =>
                    type.GetInterfaces()
                        .Any(@interface => IsElasticsearchDocumentConfigure(@interface))
                )
                .Select(type =>
                    (
                        type,
                        iType: type.GetInterfaces()
                            .First(@interface => IsElasticsearchDocumentConfigure(@interface))
                            .GenericTypeArguments[0]
                    )
                ),
        ];
    }

    private static bool IsElasticsearchDocumentConfigure(Type @interface)
    {
        return @interface.IsGenericType
            && @interface.GetGenericTypeDefinition() == typeof(IElasticsearchDocumentConfigure<>);
    }

    private static MethodInfo? GetConfigureMethod(Type type)
    {
        return type.GetMethod(nameof(IElasticsearchDocumentConfigure<string>.Configure));
    }

    private static object? CreateElasticsearchConfigBuilder(Type documentType)
    {
        var builderType = typeof(ElasticsearchConfigBuilder<>).MakeGenericType(documentType);
        return Activator.CreateInstance(builderType);
    }
}
