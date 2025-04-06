using System.Reflection;
using Elastic.Clients.Elasticsearch;
using FluentConfiguration.Configurations;

namespace FluentConfiguration;

public class ElasticsearchRegisterHelper
{
    /// <summary>
    /// Execute connection mapping config
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

            var evaluateMethodInfo = typeof(ConnectionSettingEvaluator)
                .GetMethod(nameof(IEvaluatorSync.Evaluate))!
                .MakeGenericMethod(configure.Type);

            evaluateMethodInfo.Invoke(connectionSettingEvaluator, [configure.Configs]);
        }
    }

    /// <summary>
    /// execute config classes by reflection
    /// </summary>
    /// <param name="elasticClient"></param>
    /// <param name="elsConfigs"></param>
    /// <returns></returns>
    public static async Task ElasticFluentConfigAsync(
        ElasticsearchClient elasticClient,
        IEnumerable<ElasticConfigureResult> configures
    )
    {
        foreach (var configure in configures)
        {
            object? elasticsearchClientEvaluator = Activator.CreateInstance(
                typeof(ElasticsearchClientEvaluator),
                [elasticClient]
            );

            var evaluateMethodInfo = typeof(ElasticsearchClientEvaluator)
                .GetMethod(nameof(IEvaluator.Evaluate))!
                .MakeGenericMethod(configure.Type);

            await (Task)
                evaluateMethodInfo.Invoke(elasticsearchClientEvaluator, [configure.Configs])!;
        }
    }

    /// <summary>
    /// get all of config classes by reflection
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IEnumerable<ElasticConfigureResult> GetElasticsearchConfigBuilder(
        Assembly assembly,
        string prefix
    )
    {
        var configuringTypes = GetConfiguringTypes(assembly);

        foreach (var (type, iType) in configuringTypes)
        {
            var method = GetConfigureMethod(type);
            if (method == null)
                continue;

            var elasticsearchConfigBuilder = CreateElasticsearchConfigBuilder(iType);
            var elsConfig = Activator.CreateInstance(type);

            method.Invoke(elsConfig, [elasticsearchConfigBuilder, prefix]);

            yield return new ElasticConfigureResult(elasticsearchConfigBuilder!, iType);
        }
    }

    private static IEnumerable<(Type type, Type iType)> GetConfiguringTypes(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type =>
                type.GetInterfaces().Any(@interface => IsElasticsearchDocumentConfigure(@interface))
            )
            .Select(type =>
                (
                    type,
                    iType: type.GetInterfaces()
                        .First(@interface => IsElasticsearchDocumentConfigure(@interface))
                        .GenericTypeArguments[0]
                )
            );
    }

    private static bool IsElasticsearchDocumentConfigure(Type @interface)
    {
        return @interface.IsGenericType
            && @interface.GetGenericTypeDefinition() == typeof(IElasticsearchDocumentConfigure<>);
    }

    private static MethodInfo? GetConfigureMethod(Type type)
    {
        return type.GetMethod(nameof(IElasticsearchDocumentConfigure<object>.Configure));
    }

    private static object CreateElasticsearchConfigBuilder(Type documentType)
    {
        var builderType = typeof(ElasticsearchConfigBuilder<>).MakeGenericType(documentType);
        return Activator.CreateInstance(builderType)!;
    }
}
