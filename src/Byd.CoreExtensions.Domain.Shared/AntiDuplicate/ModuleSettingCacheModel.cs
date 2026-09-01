namespace Byd.CoreExtensions.AntiDuplicate;

/// <summary>
/// Mukerrer istek korumasinin cache'te tuttugu modul ayari alanlarini tasir.
/// </summary>
// islevi: ModuleSetting satirindan yalniz koruma icin gereken alanlari projekte eder.
// sistemdeki gorevi: Ayar tablosu okumasini TTL boyunca tekrar etmeden korumaya sade ve serilestirilebilir veri saglar.
public sealed class ModuleSettingCacheModel
{
    public bool IsEnabled { get; set; }
    public bool IdempotencyEnabled { get; set; }
    public int IdempotencyTtlSeconds { get; set; }
}
