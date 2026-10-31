using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.HealthChecks;

/// <summary>
/// Redis'e ulasilip ulasilamadigini yoklar.
/// </summary>
// islevi: Paylasilan baglantinin veritabanina ping atar.
// sistemdeki gorevi: Saglik ucunda dagitik onbellegin durumunu raporlar; yalniz Redis'i acik host'ta kaydedilir.
public class RedisHealthCheck(IConnectionMultiplexer connection) : IHealthCheck, ITransientDependency
{
    // Ping cevap verirse saglikli, vermezse sagliksiz doner.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await connection.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is RedisException or RedisTimeoutException)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
