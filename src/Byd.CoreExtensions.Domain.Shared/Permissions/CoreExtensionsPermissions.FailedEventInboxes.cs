namespace Byd.CoreExtensions.Permissions;

// islevi: Basarisiz olay kayitlarinin okuma yetki sabitlerini tanimlar.
// sistemdeki gorevi: Controller action'lari yetki string'ini buradan okur; inline string kullanilmaz.
public partial class CoreExtensionsPermissions
{
    public static class FailedEventInboxes
    {
        public const string Default = GroupName + ".FailedEventInboxes";

        // Basarisiz olay detayi, filtreli listesi ve sayimini okuma yetkisi.
        public const string View = Default + ".View";
    }
}
