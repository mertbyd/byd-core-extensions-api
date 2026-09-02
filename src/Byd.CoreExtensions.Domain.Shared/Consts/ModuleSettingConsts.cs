namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Modul ayari tablo, alan siniri ve varsayilan degerlerini tanimlar.
/// </summary>
// islevi: ModuleSetting kalici modeli ile public girdi kurallarinin ortak sabitlerini tek yerde tutar.
// sistemdeki gorevi: Domain, validation ve EF konfigurasyonunun ayni kararli sinirlari kullanmasini saglar.
public static class ModuleSettingConsts
{
    public const string TableName = "ModuleSettings";
    public const string ModuleScopeIndex = "IX_ModuleSettings_ModuleName_TenantId";
    public const int MaxModuleNameLength = 128;
    public const bool DefaultIsEnabled = true;
    public const int DefaultTtlSeconds = 10;
    public const bool DefaultRateLimitEnabled = true;
    public const int DefaultRateLimitPermitLimit = 300;
    public const int DefaultRateLimitWindowSeconds = 60;
    public const int MinTtlSeconds = 1;
    public const int MaxTtlSeconds = 86400;
    public const int MinPermitLimit = 1;
    public const int MaxPermitLimit = 1000000;
    public const int MinWindowSeconds = 1;
    public const int MaxWindowSeconds = 86400;
}
