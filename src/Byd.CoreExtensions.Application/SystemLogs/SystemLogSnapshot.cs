using Serilog.Events;

namespace Byd.CoreExtensions.SystemLogs;

/// <summary>
/// Sistem log akisinin bir anda okunan ayarlarinin goruntusu.
/// </summary>
// islevi: Elasticsearch'e yazmanin acik olup olmadigini, baglanti bilgisini, veri akisi adinin parcalarini, en dusuk seviyeyi ve
// akis tanilarinin nereye yazilacagini tasir.
// sistemdeki gorevi: Isci her turda yeni goruntuyu oncekiyle karsilastirir; kayit esitligi sayesinde logger yalniz ayar
// gercekten degistiginde yeniden kurulur.
public sealed record SystemLogSnapshot(
    bool IsElasticsearchEnabled,
    string? Url,
    string? UserName,
    string? Password,
    string Dataset,
    string EnvironmentName,
    LogEventLevel MinimumLevel,
    bool IsSelfLogEnabled,
    string? FailureLogPath);
