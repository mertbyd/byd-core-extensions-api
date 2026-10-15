namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Onbellege alinan API yanitlarinin anahtar bicimi ve varsayilanlari.
/// </summary>
// islevi: Yanit anahtarinin kokunu, parca ayiricisini, kapsamsiz parca yerine yazilan isareti ve varsayilan omru tanimlar.
// sistemdeki gorevi: Yaniti yazan filtre ile grubu temizleyen depo ayni anahtar bicimini buradan kurar; desenle silme bu
// bicime dayanir.
public static class CachedResponseConsts
{
    // Yanit anahtarlarinin koku; diger onbellek kayitlariyla karismasin diye ayri tutulur.
    public const string KeyRoot = "response";

    // Anahtar parcalarinin ayiricisi.
    public const char KeySeparator = ':';

    // Tenant, organizasyon ya da kullanici olmayan istekte o parcanin yerine yazilan isaret.
    public const string NoScope = "-";

    // Omru verilmeyen yanitin onbellekte kalma suresi (dakika).
    public const int DefaultDurationMinutes = 10;

    // Onbellekten donen yaniti isaretleyen basligin adi.
    public const string CacheHeaderName = "X-Response-Cache";

    // Onbellekten donen yanitin baslik degeri.
    public const string CacheHitValue = "HIT";
}
