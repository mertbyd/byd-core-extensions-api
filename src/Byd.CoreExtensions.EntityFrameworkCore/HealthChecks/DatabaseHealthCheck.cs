using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.HealthChecks;

/// <summary>
/// Default baglanti dizesindeki PostgreSQL'e baglanilip baglanilamadigini yoklar.
/// </summary>
// islevi: Baglanti dizesini ABP'nin cozucusuyle alir, baglanti acar ve hemen kapatir.
// sistemdeki gorevi: Saglik ucunda veritabaninin durumunu raporlar; unit of work ve DbContext acmadan calisir.
public class DatabaseHealthCheck(IConnectionStringResolver connectionStringResolver) : IHealthCheck, ITransientDependency
{
    // Baglanti acilirsa saglikli, acilamazsa sagliksiz doner.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(await connectionStringResolver.ResolveAsync());
        try
        {
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (NpgsqlException exception)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
