namespace Byd.CoreExtensions.Localization;

/// <summary>
/// Modulun localization kaynak adlarini ve sanal klasor yollarini tasiyan sabit sahibi.
/// </summary>
// islevi: Kaynak adi, sanal yol ve enum anahtar bicimi gibi kararli localization dizgelerini tek yerde toplar.
// sistemdeki gorevi: Ozniteliklerin, modul kaydinin ve saglayicilarin ayni dizgeyi tekrar yazmasini engeller; ad degisirse tek dosya guncellenir.
public static class CoreExtensionsLocalizationConsts
{
    // Kaynaklarin kayit edildigi varsayilan kultur.
    public const string DefaultCulture = "en";

    // Modulun genel localization kaynaginin adi.
    public const string ResourceName = "CoreExtensions";

    // Genel kaynagin JSON dosyalarinin arandigi gomulu klasor yolu.
    public const string VirtualPath = "/Localization/" + ResourceName;

    // Enum lokalizasyon anahtarinin kararli bicimi: tip adi ve deger adi noktayla birlesir.
    public const string EnumKeyFormat = "{0}.{1}";

    /// <summary>
    /// Izin tanimlarinin gosterim adlarini tasiyan localization anahtarlari.
    /// </summary>
    public static class Permissions
    {
        public const string Group = "Permission:CoreExtensions";
        public const string ModuleSettings = "Permission:ModuleSettings";
        public const string ModuleSettingsView = "Permission:ModuleSettings.View";
        public const string ModuleSettingsManage = "Permission:ModuleSettings.Manage";
        public const string ApiKeys = "Permission:ApiKeys";
        public const string ApiKeysView = "Permission:ApiKeys.View";
        public const string ApiKeysManage = "Permission:ApiKeys.Manage";
        public const string FailedEventInboxes = "Permission:FailedEventInboxes";
        public const string FailedEventInboxesView = "Permission:FailedEventInboxes.View";
    }

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
