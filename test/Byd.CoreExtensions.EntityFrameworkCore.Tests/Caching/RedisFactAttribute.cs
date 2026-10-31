using System;
using Xunit;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// Calisan bir Redis'e ihtiyac duyan testi, Redis ayarlanmamis makinede atlar.
/// </summary>
// islevi: Modulun okudugu Redis baglanti dizesi ortam degiskeninde yoksa testi atlanmis isaretler.
// sistemdeki gorevi: Onbellek testleri gercek Redis'e karsi kosar; Redis'i olmayan gelistiricide ya da CI'da kirmiziya dusmez.
public class RedisFactAttribute : FactAttribute
{
    // Redis:Configuration anahtarinin ortam degiskeni karsiligi.
    public const string ConfigurationVariable = "Redis__Configuration";

    public RedisFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConfigurationVariable)))
            Skip = $"{ConfigurationVariable} is not set; cache tests need a running Redis.";
    }
}
