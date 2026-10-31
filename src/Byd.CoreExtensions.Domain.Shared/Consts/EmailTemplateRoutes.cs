namespace Byd.CoreExtensions.Consts;

/// <summary>
/// E-posta sablonu HTTP rotalarini tanimlar.
/// </summary>
// islevi: EmailTemplate CRUD ve aktiflestirme endpointlerinin kararli rota sozlesmesini tek yerde tutar.
// sistemdeki gorevi: Controller ile istemcilerin ayni derleme guvenli route degerlerini kullanmasini saglar.
public static class EmailTemplateRoutes
{
    public const string Root = "api/core-extensions/email-templates";
    public const string ById = "{id:guid}";
    public const string ActivateById = "{id:guid}/activate";
}
