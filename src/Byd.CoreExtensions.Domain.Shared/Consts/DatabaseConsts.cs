namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Modulun veritabani sema adlarini tanimlar.
/// </summary>
// islevi: Modulun kullandigi tum sema adlarini derleme guvenli tek sabit kumesinde tutar.
// sistemdeki gorevi: Modul bir ana uygulamaya baglandiginda hicbir tablo varsayilan public semasini
// kirletmesin diye sema adlarinin tek kaynagidir; sema cozumu, EF konfigurasyonu ve migration ayni sabitleri okur.
public static class DatabaseConsts
{
    // ABP framework tablolarinin (ayar, izin, denetim vb.) yazildigi sema.
    public const string AbpSchema = "abp";

    // Modulun rate limit yapilandirma tablolarinin yazildigi sema.
    public const string RateLimitSchema = "ratelimit";
}
