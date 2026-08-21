namespace Byd.CoreExtensions.Localization;

/// <summary>
/// Modulun localization kaynak adlarini ve sanal klasor yollarini tasiyan sabit sahibi.
/// </summary>
// islevi: Kaynak adi, sanal yol ve enum anahtar bicimi gibi kararli localization dizgelerini tek yerde toplar.
// sistemdeki gorevi: Ozniteliklerin, modul kaydinin ve saglayicilarin ayni dizgeyi tekrar yazmasini engeller; ad degisirse tek dosya guncellenir.
public static class CoreExtensionsLocalizationConsts
{
    // Modulun genel localization kaynaginin adi.
    public const string ResourceName = "CoreExtensions";

    // Genel kaynagin JSON dosyalarinin arandigi gomulu klasor yolu.
    public const string VirtualPath = "/Localization/" + ResourceName;

    // Enum lokalizasyon anahtarinin kararli bicimi: tip adi ve deger adi noktayla birlesir.
    public const string EnumKeyFormat = "{0}.{1}";

    /// <summary>
    /// Postgres saglayici hatalarina ayrilmis ikinci kaynagin sabitleri.
    /// </summary>
    public static class PostgresExceptions
    {
        // Postgres hata kaynaginin adi.
        public const string ResourceName = "CoreExtensionsPostgresExceptions";

        // Postgres hata kaynaginin JSON dosyalarinin arandigi gomulu klasor yolu.
        public const string VirtualPath = "/Localization/" + ResourceName;
    }
}
