using System;
using Byd.CoreExtensions.Settings;
using Xunit;

namespace Byd.CoreExtensions.Elasticsearch;

/// <summary>
/// Calisan bir Elasticsearch kumesine ihtiyac duyan testi, kume ayarlanmamis makinede atlar.
/// </summary>
// islevi: ABP'nin ortam degiskeninden okudugu Elasticsearch adres ayari yoksa testi atlanmis isaretler.
// sistemdeki gorevi: Arama testleri gercek kumeye karsi kosar; kumesi olmayan gelistiricide ya da CI'da kirmiziya dusmez.
public class ElasticsearchFactAttribute : FactAttribute
{
    // ABP ayarlarinin ortam degiskeni karsiligi: Settings__<ayar adi>.
    public const string UrlVariable = "Settings__" + CoreExtensionsSettings.Elasticsearch.Url;

    public ElasticsearchFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(UrlVariable)))
            Skip = $"{UrlVariable} is not set; Elasticsearch tests need a running cluster.";
    }
}
