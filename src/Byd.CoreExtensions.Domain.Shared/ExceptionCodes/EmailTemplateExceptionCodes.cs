using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions.ExceptionCodes;

/// <summary>
/// E-posta sablonu is ve girdi dogrulama hata kodlarini tanimlar.
/// </summary>
// islevi: EmailTemplate validation, aktiflik ve sablon cozumleme hatalarina kararli kodlar verir.
// sistemdeki gorevi: Validator, Manager, localization ve istemciler icin tek hata sozlesmesi sahibidir.
public static class EmailTemplateExceptionCodes
{
    public const string NameRequired = ExceptionCodeConsts.EmailTemplateGroup + "0001";
    public const string NameTooLong = ExceptionCodeConsts.EmailTemplateGroup + "0002";
    public const string CultureNameRequired = ExceptionCodeConsts.EmailTemplateGroup + "0003";
    public const string CultureNameTooLong = ExceptionCodeConsts.EmailTemplateGroup + "0004";
    public const string SubjectRequired = ExceptionCodeConsts.EmailTemplateGroup + "0005";
    public const string SubjectTooLong = ExceptionCodeConsts.EmailTemplateGroup + "0006";
    public const string BodyRequired = ExceptionCodeConsts.EmailTemplateGroup + "0007";
    public const string EmailEventRequired = ExceptionCodeConsts.EmailTemplateGroup + "0008";
    public const string InvalidTemplateSyntax = ExceptionCodeConsts.EmailTemplateGroup + "0009";
    public const string TemplateNotFound = ExceptionCodeConsts.EmailTemplateGroup + "0010";
    public const string ActiveTemplateCannotBeDeleted = ExceptionCodeConsts.EmailTemplateGroup + "0011";
    public const string TemplateAlreadyActive = ExceptionCodeConsts.EmailTemplateGroup + "0012";
}
