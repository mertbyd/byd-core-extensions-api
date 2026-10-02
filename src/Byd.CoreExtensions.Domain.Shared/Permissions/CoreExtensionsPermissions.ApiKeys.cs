namespace Byd.CoreExtensions.Permissions;

// islevi: API anahtarlarinin okuma ve uretme yetki sabitlerini tanimlar.
// sistemdeki gorevi: Controller action'lari yetki string'ini buradan okur; inline string kullanilmaz.
public partial class CoreExtensionsPermissions
{
    public static class ApiKeys
    {
        public const string Default = GroupName + ".ApiKeys";

        // API anahtari detayini okuma yetkisi.
        public const string View = Default + ".View";

        // Yeni API anahtari uretme yetkisi.
        public const string Manage = Default + ".Manage";
    }
}
