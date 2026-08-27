namespace Byd.CoreExtensions.ApiKeys;

/// <summary>
/// Icinde bulunulan istegi dogrulayan API anahtarinin request kapsamli baglami.
/// </summary>
// islevi: Basarili dogrulama sonrasi cozulmus anahtar goruntusunu istek boyunca tasir.
// sistemdeki gorevi: "Bu istegi hangi API anahtari yapti" bilgisinin downstream kod tarafindan okundugu tek nokta; IRateLimitState.Current deseninin API anahtari karsiligi.
public interface ICurrentApiKey
{
    // Icinde bulunulan istegi dogrulayan anahtar goruntusu; dogrulama yapilmadiysa null.
    ApiKeyDescriptor? Current { get; }
}
