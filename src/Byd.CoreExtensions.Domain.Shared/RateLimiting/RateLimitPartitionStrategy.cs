namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Bir endpoint'in rate limit sayacini bolebilecegi partition strateji belirtecleri.
/// </summary>
// islevi: Sayac ayrimini (IP, kullanici, tenant, client ve bilesimleri) derleme guvenli sabit metinler olarak tanimlar.
// sistemdeki gorevi: Gecerli strateji kumesinin tek sahibi; partition cozucu gelen metni bu kumeye karsi dogrular, kimlik bulunamazsa daima IP'ye duser.
public static class RateLimitPartitionStrategy
{
    // Istemci IP adresi; anonim istekler icin varsayilan.
    public const string Ip = "ip"; //TODO BİLİYORUM

    // Kimlik dogrulanmis kullanici (UserId).
    public const string User = "user"; //TODO BİLİYORUM

    // SaaS kiracisi (TenantId).
    public const string Tenant = "tenant";//TODO BİLİYORUM

    // Harici API tuketicisi (ClientId / API anahtari).
    public const string Client = "client";//TODO BİLMİYORUM

    // Kullanici ve endpoint bilesimi; endpoint basina ayri sayac.
    public const string UserAndEndpoint = "userAndEndpoint";//TODO BİLMİYORUM

    // Tenant ve kullanici bilesimi; kiracinin ic kullanicisi bazinda sayac.
    public const string TenantAndUser = "tenantAndUser";//TODO BİLMİYORUM
}
