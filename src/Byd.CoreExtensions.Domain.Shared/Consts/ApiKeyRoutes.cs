namespace Byd.CoreExtensions.Consts;

/// <summary>
/// API anahtari HTTP rotalarini tanimlar.
/// </summary>
// islevi: ApiKey CRUD endpointlerinin kararli rota sozlesmesini tek yerde tutar.
// sistemdeki gorevi: Controller ile istemcilerin ayni derleme guvenli route degerlerini kullanmasini saglar.
public static class ApiKeyRoutes
{
    public const string Root = "api/core-extensions/api-keys";
    public const string ById = "{id:guid}";
}
