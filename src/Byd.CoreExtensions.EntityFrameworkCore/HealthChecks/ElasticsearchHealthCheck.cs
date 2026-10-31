using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Providers;
using Byd.CoreExtensions.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Byd.CoreExtensions.HealthChecks;

/// <summary>
/// Elasticsearch kumesine ulasilip ulasilamadigini yoklar.
/// </summary>
// islevi: Ayarlarda kapali olan Elasticsearch'i saglikli sayar; acikken kumeye ping atar.
// sistemdeki gorevi: Saglik ucunda arama altyapisinin durumunu raporlar; istemciyi repository ile ayni saglayicidan alir.
public class ElasticsearchHealthCheck(ISettingProvider settingProvider, ElasticsearchClientProvider clientProvider)
    : IHealthCheck, ITransientDependency
{
    // Kapaliysa saglikli, aciksa ping sonucuna gore saglikli ya da sagliksiz doner.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!await settingProvider.IsTrueAsync(CoreExtensionsSettings.Elasticsearch.IsEnabled))
            return HealthCheckResult.Healthy(HealthCheckConsts.DisabledDescription);
        var client = await clientProvider.GetClientAsync(cancellationToken);
        try
        {
            var response = await client.PingAsync(new PingRequest(), cancellationToken);
            return response.IsValidResponse ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (TransportException exception)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
