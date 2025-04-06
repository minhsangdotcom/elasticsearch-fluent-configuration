Fluent Configurations for elasticsearch in c#.

Let check the source code out at [Github](https://github.com/minhsangdotcom/elasticsearch-fluent-configuration)

Check out My clean architecture solution template at [Github](https://github.com/minhsangdotcom/Clean-Architecture_The-Template)

Work well with [Elastic.Clients.Elasticsearch](https://www.nuget.org/packages/Elastic.Clients.Elasticsearch) package

# Usage Example

```csharp
public class AuditLogConfiguration : IElasticsearchDocumentConfigure<AuditLog>
{
    public void Configure(ref ElasticsearchConfigBuilder<AuditLog> buider, string? prefix = null)
    {
        // declare the name of index
        buider.ToIndex("audit_log");

        // set key
        buider.HasKey(key => key.Id);

        // add settings
        buider.Settings(setting =>
            setting.Analysis(x =>
                x.Analyzers(an =>
                        an.Custom(
                                "myTokenizer",
                                ca => ca.Filter(["lowercase"]).Tokenizer("myTokenizer")
                            )
                            .Custom(
                                "standardAnalyzer",
                                ca => ca.Filter(["lowercase"]).Tokenizer("standard")
                            )
                    )
                    .Tokenizers(tz =>
                        tz.NGram(
                            "myTokenizer",
                            config =>
                                config
                                    .MinGram(3)
                                    .MaxGram(4)
                                    .TokenChars([TokenChar.Digit, TokenChar.Letter])
                        )
                    )
            )
        );

        // Map properties Manually
        buider.Properties(config =>
            config
                .Text(
                    t => t.Id,
                    config =>
                        config
                            .Fields(f =>
                                f.Keyword("Id")
                            )
                            .Analyzer("myTokenizer")
                            .SearchAnalyzer("standardAnalyzer")
                )
                .Text(
                    txt => txt.Entity,
                    config =>
                        config
                            .Fields(f =>
                                f.Keyword("Entity")
                            )
                            .Analyzer("myTokenizer")
                            .SearchAnalyzer("standardAnalyzer")
                )
                .ByteNumber(b => b.Type)
                .Object(o => o.OldValue!)
                .Object(o => o.NewValue!)
                .Text(txt => txt.ActionPerformBy!)
                .Keyword(d => d.CreatedAt)
        );

        // Ignore properties
        buider.Ignores([x => x.NewValue!, x => x.Type]);
    }
}
```

# Register

```csharp
ElasticsearchSettings elasticsearch =
    configuration.GetSection(nameof(ElasticsearchSettings)).Get<ElasticsearchSettings>()
            ?? new();

    if (elasticsearch.IsEnbaled)
    {
        IEnumerable<Uri> nodes = elasticsearch!.Nodes.Select(x => new Uri(x));
        var pool = new StaticNodePool(nodes);
        string? userName = elasticsearch.Username;
        string? password = elasticsearch.Password;

        var settings = new ElasticsearchClientSettings(pool).DefaultIndex(
            elasticsearch.DefaultIndex!
        );

        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
        {
            settings
                .Authentication(new BasicAuthentication(userName, password))
                // without ssl trust
                .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
        }

        IEnumerable<ElasticConfigureResult> elkConfigbuilder =
            ElasticsearchRegisterHelper.GetElasticsearchConfigBuilder(
                Assembly.GetExecutingAssembly(),
                elasticsearch.PrefixIndex!
            );

        // add configurations of id, ignore properties
        ElasticsearchRegisterHelper.ConfigureConnectionSettings(ref settings, elkConfigbuilder);

        var client = new ElasticsearchClient(settings);

        // add configuration of properties
        await ElasticsearchRegisterHelper.ElasticFluentConfigAsync(
            elasticsearchClient,
            configures
        );

        await DataSeeding.SeedingAsync(client, elasticsearch.PrefixIndex)

        services
            .AddSingleton(client)
            .AddSingleton<IElasticsearchServiceFactory, ElasticsearchServiceFactory>();
    }
```
