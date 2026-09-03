using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Modul ayari is ve girdi dogrulama hata kodlarini tanimlar.
/// </summary>
// islevi: ModuleSetting validation ve benzersizlik hatalarina kararli kodlar verir.
// sistemdeki gorevi: Validator, localization ve ilerideki istemciler icin tek hata sozlesmesi sahibidir.
public static class ModuleSettingExceptionCodes
{
    public const string Prefix = ExceptionCodeConsts.RootNamespace + ".ModuleSetting";
    public const string ModuleNameRequired = Prefix + ":00001";
    public const string ModuleNameTooLong = Prefix + ":00002";
    public const string DuplicateModuleName = Prefix + ":00003";
    public const string InvalidTtlSeconds = Prefix + ":00004";
    public const string InvalidRateLimitPermitLimit = Prefix + ":00005";
    public const string InvalidRateLimitWindowSeconds = Prefix + ":00006";
    public const string InvalidIdempotencyTtlSeconds = Prefix + ":00007";
    public const string ModuleNameNotConfigured = Prefix + ":00008";
    public const string PipelineNotConfigured = Prefix + ":00009";
    public const string InvalidFailedEventRetryWindowSeconds = Prefix + ":00010";
    public const string EnvironmentNameNotConfigured = Prefix + ":00011";
}
