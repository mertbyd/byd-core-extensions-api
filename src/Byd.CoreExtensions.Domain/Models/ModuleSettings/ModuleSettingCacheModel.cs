namespace Byd.CoreExtensions.Models.ModuleSettings;

/// <summary>
/// Idempotency filtresinin cache'te tuttugu modul ayari alanlarini tasir.
/// </summary>
// islevi: ModuleSetting entity'sinden yalniz idempotency icin gereken alanlari projekte eder.
// sistemdeki gorevi: Ayar tablosu okumasini TTL boyunca tekrar etmeden filtreye sade ve serilestirilebilir veri saglar.
public sealed class ModuleSettingCacheModel
{
    public bool IsEnabled { get; set; }
    public bool IdempotencyEnabled { get; set; }
    public int IdempotencyTtlSeconds { get; set; }
}
