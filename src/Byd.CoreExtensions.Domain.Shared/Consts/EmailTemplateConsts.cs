namespace Byd.CoreExtensions.Consts;

/// <summary>
/// E-posta sablonu tablo, indeks ve alan siniri degerlerini tanimlar.
/// </summary>
// islevi: EmailTemplate kalici modeli ile public girdi kurallarinin ortak sabitlerini tek yerde tutar.
// sistemdeki gorevi: Domain, validation ve EF konfigurasyonunun ayni kararli sinirlari kullanmasini saglar.
public static class EmailTemplateConsts
{
    public const string TableName = "EmailTemplates";
    public const string ScopeIndex = "IX_EmailTemplates_EmailEventId_CultureName_TenantId_OrganizationId";
    public const int MaxNameLength = 128;
    public const int MaxCultureNameLength = 10;
    public const int MaxSubjectLength = 256;
}
