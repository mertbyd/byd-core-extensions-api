namespace Byd.CoreExtensions.Dtos.ApiKeys;

/// <summary>
/// Yeni olusturulan API anahtarinin, uretilen anahtar degeriyle birlikte cevabini tasir.
/// </summary>
// islevi: Kayit alanlarina ek olarak uretilen ham anahtar degerini tek seferlik olarak doner.
// sistemdeki gorevi: Anahtarin istemciye gosterildigi tek noktadir; get ve list cevaplari anahtari tasimaz.
public sealed class CreatedApiKeyDto : ApiKeyDto
{
    public string Key { get; set; } = default!;
}
