namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Modulun veritabani sema adlarini tanimlar.
/// </summary>
// islevi: ABP framework tablolarinin hangi semaya yazilacagini derleme guvenli tek sabitte tutar.
// sistemdeki gorevi: Modul bir ana uygulamaya baglandiginda ABP tablolari varsayilan public semasini
// kirletmesin diye sema adinin tek kaynagidir; sema cozumu ve migration ayni sabiti okur.
public static class DatabaseConsts
{
    // ABP framework tablolarinin (ayar, izin, denetim vb.) yazildigi sema.
    public const string AbpSchema = "abp";
}
