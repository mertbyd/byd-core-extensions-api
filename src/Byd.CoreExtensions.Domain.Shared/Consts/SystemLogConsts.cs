namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Host'larin sistem log yapilandirmasinin kararli degerleri.
/// </summary>
// islevi: Log dosyasinin yolunu, seviyesi ayrica belirlenen kaynaklari, Elasticsearch veri akisinin turunu ve ayar varsayilanlarini
// tanimlar.
// sistemdeki gorevi: Host'larin Program.cs'teki baslangic logger'i ile ayarlara gore log akisini yeniden kuran isci ayni
// degerleri bu tek kaynaktan okur; boylece iki yapilandirma ayrisamaz.
public static class SystemLogConsts
{
    // Host'un yerel log dosyasi.
    public const string FilePath = "Logs/logs.txt";

    // Seviyesi Information'a cekilen cerceve kaynaginin on eki.
    public const string MicrosoftSource = "Microsoft";

    // Seviyesi Warning'e cekilen EF Core kaynaginin on eki.
    public const string EntityFrameworkCoreSource = "Microsoft.EntityFrameworkCore";

    // Sistem loglarinin yazildigi veri akisinin turu; ad "logs-<indeks>-<ortam>" bicimini alir.
    public const string DataStreamType = "logs";

    // Veri akisi adinda parca ayiricisi oldugu icin indeks ve ortam adlarinda yerine yazilan karakter.
    public const char DataStreamSeparatorReplacement = '.';

    // Veri akisi adinin parca ayiricisi.
    public const char DataStreamSeparator = '-';

    // Sistem loglarinin Elasticsearch'e yazildigi varsayilan en dusuk seviye.
    public const string DefaultMinimumLevel = "Information";

    // Sistem log ayarlarinin yeniden okunma araliginin varsayilani (saniye).
    public const int DefaultRefreshIntervalSeconds = 60;
}
