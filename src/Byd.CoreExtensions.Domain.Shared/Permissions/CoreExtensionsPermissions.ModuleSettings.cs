namespace Byd.CoreExtensions.Permissions;

// islevi: Modul ayari kayitlarinin okuma ve yonetim yetki sabitlerini tanimlar.
// sistemdeki gorevi: Controller action'lari yetki string'ini buradan okur; inline string kullanilmaz.
public partial class CoreExtensionsPermissions
{
    public static class ModuleSettings
    {
        public const string Default = GroupName + ".ModuleSettings";

        // Modul ayari listesi, detayi ve rate limit, idempotency, olay kutusu alanlarini okuma yetkisi.
        public const string View = Default + ".View";

        // Modul ayari olusturma, guncelleme ve silme yetkisi.
        public const string Manage = Default + ".Manage";
    }
}
