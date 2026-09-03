namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Olay kutusu korumasinin ve basarisiz olay yeniden deneme penceresinin sabit degerlerini tanimlar.
/// </summary>
// islevi: Olay kutusu etkinligini ve basarisiz olay yeniden deneme ayarlarinin varsayilanlari ile kabul edilen sure araligini tek yerde tutar.
// sistemdeki gorevi: ModuleSetting validation, EF konfigurasyonu ve olay kutusu tuketicilerinin ortak sozlesmesidir.
public static class EventBoxConsts
{
    // Yeni acilan modul satirinda olay kutusu korumasinin acik gelip gelmedigi.
    public const bool DefaultEnabled = true;

    // Basarisiz olaylarin varsayilan olarak suresiz yeniden denenip denenmedigi.
    public const bool DefaultFailedEventRetryUnlimited = false;

    // Basarisiz bir olayin varsayilan yeniden deneme penceresi (saniye).
    public const int DefaultFailedEventRetryWindowSeconds = 86400;

    // Ayarda kabul edilen en kisa basarisiz olay yeniden deneme penceresi (saniye).
    public const int MinFailedEventRetryWindowSeconds = 1;

    // Ayarda kabul edilen en uzun basarisiz olay yeniden deneme penceresi (saniye).
    public const int MaxFailedEventRetryWindowSeconds = 604800;

    // Cercevenin gelen olay kutusunda bir kayittan vazgecmeden once yapacagi azami deneme sayisi.
    // Esik asildiginda kayit vazgecilmis sayilir ve modulun kalici arsivine kopyalanir.
    public const int InboxProcessorMaxRetryCount = 2;

    // Gelen olay kutusunun tablo adi.
    //todo : olay kutusu tablolari diger modul tablolarinin aksine core_extensions_ on eki almadi,
    //       ilerleyen asamada on ekli adlandirma denenecek; denenip vazgecilirse bu not silinebilir
    public const string InboxTableName = "EventInbox";

    // Giden olay kutusunun tablo adi.
    public const string OutboxTableName = "EventOutbox";

    // Kaydi yazan kurulumun modul adini tasiyan ozelligin kararli adi.
    public const string OwnerModuleNamePropertyName = "OwnerModuleName";

    // Kaydi yazan kurulumun ortam adini tasiyan ozelligin kararli adi.
    public const string OwnerEnvironmentNamePropertyName = "OwnerEnvironmentName";

    // Kayit sahibi modul adi ile modul ayari satiri ayni beyana dayanir; sinirlar ayrisirsa kabul edilen modul adi kutu kolonuna sigmayabilir.
    public const int MaxOwnerModuleNameLength = ModuleSettingConsts.MaxModuleNameLength;

    // Kayit sahibi ortam adi icin izin verilen en uzun karakter sayisi.
    public const int MaxOwnerEnvironmentNameLength = 64;
}
