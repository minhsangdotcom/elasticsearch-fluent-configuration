# Els Fluent configurations

Fluent Configurations for elasticsearch in c#.

**Example**
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
Let check the source code out at [Nuget](https://www.nuget.org/packages/minhsangdotcom.TheTemplate.ElasticsearchFluentConfig)
