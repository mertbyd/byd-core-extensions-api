namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Sunucunun dogruladigi oturum seviyesini tasiyan claim adlari.
/// </summary>
// islevi: Token ureticisi ile kapsam tuketicisinin ortak claim sozlesmesini tanimlar.
// sistemdeki gorevi: Oturum seviyesinin tenant ve organizasyon kimliklerinden tahmin edilmesini onler.
public static class ScopeClaimTypes
{
    public const string Level = "scope_level";
}
