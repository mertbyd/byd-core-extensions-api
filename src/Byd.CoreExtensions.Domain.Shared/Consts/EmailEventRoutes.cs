namespace Byd.CoreExtensions.Consts;

/// <summary>
/// E-posta olayi HTTP rotalarini tanimlar.
/// </summary>
// islevi: EmailEvent okuma endpointlerinin kararli rota sozlesmesini tek yerde tutar.
// sistemdeki gorevi: Controller ile istemcilerin ayni derleme guvenli route degerlerini kullanmasini saglar.
public static class EmailEventRoutes
{
    public const string Root = "api/core-extensions/email-events";
    public const string ById = "{id:guid}";
}
