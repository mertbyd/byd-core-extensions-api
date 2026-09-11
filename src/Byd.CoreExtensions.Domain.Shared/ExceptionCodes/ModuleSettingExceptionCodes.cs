using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// Modul ayari is ve girdi dogrulama hata kodlarini tanimlar.
/// </summary>
// islevi: ModuleSetting validation ve benzersizlik hatalarina kararli kodlar verir.
// sistemdeki gorevi: Validator, localization ve ilerideki istemciler icin tek hata sozlesmesi sahibidir.
public static class ModuleSettingExceptionCodes
{
    public const string ModuleNameRequired = ExceptionCodeConsts.ModuleSettingGroup + "0001";
    public const string ModuleNameTooLong = ExceptionCodeConsts.ModuleSettingGroup + "0002";
    public const string DuplicateModuleName = ExceptionCodeConsts.ModuleSettingGroup + "0003";
    public const string InvalidTtlSeconds = ExceptionCodeConsts.ModuleSettingGroup + "0004";
    public const string InvalidRateLimitPermitLimit = ExceptionCodeConsts.ModuleSettingGroup + "0005";
    public const string InvalidRateLimitWindowSeconds = ExceptionCodeConsts.ModuleSettingGroup + "0006";
    public const string InvalidIdempotencyTtlSeconds = ExceptionCodeConsts.ModuleSettingGroup + "0007";
    public const string ModuleNameNotConfigured = ExceptionCodeConsts.ModuleSettingGroup + "0008";
    public const string PipelineNotConfigured = ExceptionCodeConsts.ModuleSettingGroup + "0009";
    public const string InvalidFailedEventRetryWindowSeconds = ExceptionCodeConsts.ModuleSettingGroup + "0010";
    public const string EnvironmentNameNotConfigured = ExceptionCodeConsts.ModuleSettingGroup + "0011";
}
