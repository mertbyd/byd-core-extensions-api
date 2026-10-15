using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client.Exceptions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.RabbitMQ;

namespace Byd.CoreExtensions.HealthChecks;

/// <summary>
/// Dagitik olaylarin tasindigi RabbitMQ baglantisinin acik olup olmadigini yoklar.
/// </summary>
// islevi: ABP'nin baglanti havuzundaki varsayilan baglantiyi alir ve acik olup olmadigina bakar.
// sistemdeki gorevi: Saglik ucunda olay tasimasinin durumunu raporlar; olay yolunun kullandigi baglantinin kendisini yoklar.
public class RabbitMqHealthCheck(IConnectionPool connectionPool) : IHealthCheck, ITransientDependency
{
    // Baglanti aciksa saglikli, kurulamiyorsa ya da kapaliysa sagliksiz doner.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await connectionPool.GetAsync(RabbitMqConnections.DefaultConnectionName);
            return connection.IsOpen ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (BrokerUnreachableException exception)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
