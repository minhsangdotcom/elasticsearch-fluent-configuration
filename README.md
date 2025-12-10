# Els Fluent configurations

Fluent Configurations for elasticsearch in c#.

**Example**

```csharp
public class AuditLogConfiguration : IElasticsearchDocumentConfigure<AuditLog>
{
    public void Configure(ref ElasticsearchConfigBuilder<AuditLog> builder, string? prefix = null)
    {
        // declare the name of index
        builder.ToIndex(name:"audit-log", prefix : prefix);

        // set key
        builder.HasKey(key => key.Id);

        // add settings
        builder.Settings(setting =>
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
        builder.Properties(config =>
            config
                .Text(
                    t => t.Id,
                    config =>
                        config
                            .Fields(f =>
                                f.Keyword("raw")
                            )
                            .Analyzer("myTokenizer")
                            .SearchAnalyzer("standardAnalyzer")
                )
                .Text(
                    txt => txt.Entity,
                    config =>
                        config
                            .Fields(f =>
                                f.Keyword("raw")
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
        builder.Ignores([x => x.NewValue!, x => x.Type]);
    }
}
```

```
dotnet add package minhsangdotcom.TheTemplate.ElasticsearchFluentConfig
```
