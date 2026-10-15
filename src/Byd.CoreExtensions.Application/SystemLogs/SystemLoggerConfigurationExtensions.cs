using System;
using System.IO;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Microsoft.Extensions.Hosting;
using Byd.CoreExtensions.Consts;
using Serilog;
using Serilog.Events;

namespace Byd.CoreExtensions.SystemLogs;

/// <summary>
/// Host'larin sistem log yapilandirmasini kuran Serilog uzantilari.
/// </summary>
// islevi: Ev host'larinin ortak log yapilandirmasini kurar ve ayarlar aciksa Elasticsearch veri akisi hedefini ekler; gonderilemeyen
// log paketlerinin ayrintisini istenen dosyaya yazar.
// sistemdeki gorevi: Program.cs'teki baslangic logger'i ile ayarlara gore log akisini yeniden kuran isci ayni yapilandirmayi buradan
// alir; host'larda tekrar eden log blogu tek yere iner.
public static class SystemLoggerConfigurationExtensions
{
    // Gelistirmede Debug, diger ortamlarda Information seviyesiyle; cerceve kaynaklarini kisarak dosyaya ve konsola yazar.
    public static LoggerConfiguration ConfigureSystemLog(this LoggerConfiguration configuration, string environmentName)
    {
        return configuration
            .MinimumLevel.Is(environmentName == Environments.Development ? LogEventLevel.Debug : LogEventLevel.Information)
            .MinimumLevel.Override(SystemLogConsts.MicrosoftSource, LogEventLevel.Information)
            .MinimumLevel.Override(SystemLogConsts.EntityFrameworkCoreSource, LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(sink => sink.File(SystemLogConsts.FilePath))
            .WriteTo.Async(sink => sink.Console());
    }

    // Goruntu Elasticsearch'i aciyorsa "logs-<indeks>-<ortam>" veri akisina yazan hedefi ekler; kume ulasilamazsa host durmaz.
    public static LoggerConfiguration WriteToElasticsearch(this LoggerConfiguration configuration, SystemLogSnapshot snapshot)
    {
        if (!snapshot.IsElasticsearchEnabled || string.IsNullOrWhiteSpace(snapshot.Url))
            return configuration;
        return configuration.WriteTo.Elasticsearch(
            [new Uri(snapshot.Url)],
            sink =>
            {
                sink.DataStream = new DataStreamName(SystemLogConsts.DataStreamType, snapshot.Dataset, snapshot.EnvironmentName);
                sink.BootstrapMethod = BootstrapMethod.None;
                sink.MinimumLevel = snapshot.MinimumLevel;
                sink.ConfigureChannel = channel =>
                {
                    channel.ExportExceptionCallback = exception => AppendFailure(snapshot.FailureLogPath, exception.ToString());
                    channel.ServerRejectionCallback = rejected => AppendFailure(snapshot.FailureLogPath, rejected.Count.ToString());
                };
            },
            transport =>
            {
                if (!string.IsNullOrWhiteSpace(snapshot.UserName))
                    transport.Authentication(new BasicAuthentication(snapshot.UserName, snapshot.Password ?? string.Empty));
            });
    }

    // Gonderilemeyen paketin ayrintisini dosyaya ekler; dosya yazilamazsa log akisi bundan etkilenmez.
    private static void AppendFailure(string? path, string detail)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;
        try
        {
            File.AppendAllText(path, $"{DateTime.UtcNow:O} {detail}{Environment.NewLine}");
        }
        catch (IOException)
        {
            // Tani dosyasi yazilamadi; asil log akisi zaten kendi yeniden deneme yoluyla devam eder.
        }
    }
}
