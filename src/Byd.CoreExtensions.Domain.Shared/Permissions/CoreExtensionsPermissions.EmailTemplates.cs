namespace Byd.CoreExtensions.Permissions;

// islevi: E-posta sablonlarinin okuma ve yonetim yetki sabitlerini tanimlar.
// sistemdeki gorevi: Controller action'lari yetki string'ini buradan okur; inline string kullanilmaz.
public partial class CoreExtensionsPermissions
{
    public static class EmailTemplates
    {
        public const string Default = GroupName + ".EmailTemplates";

        // Kendi kapsamindaki e-posta sablonlarinin listesini ve detayini okuma yetkisi.
        public const string View = Default + ".View";

        // E-posta sablonu olusturma, guncelleme, silme ve aktiflestirme yetkisi.
        public const string Manage = Default + ".Manage";
    }
}
