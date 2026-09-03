namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Modul ayari HTTP rotalarini tanimlar.
/// </summary>
// islevi: ModuleSetting CRUD endpointlerinin kararli rota sozlesmesini tek yerde tutar.
// sistemdeki gorevi: Controller ile istemcilerin ayni derleme guvenli route degerlerini kullanmasini saglar.
public static class ModuleSettingRoutes
{
    public const string Root = "api/core-extensions/module-settings";
    public const string ById = "{id:guid}";
    public const string RateLimitById = "{id:guid}/rate-limit";
    public const string IdempotencyById = "{id:guid}/idempotency";
    public const string EventBoxById = "{id:guid}/event-box";
}
