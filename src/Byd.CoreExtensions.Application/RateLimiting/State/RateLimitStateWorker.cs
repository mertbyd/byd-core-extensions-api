using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.Consts;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Byd.CoreExtensions.RateLimiting;

/// <summary>
/// Rate limit ayarlarini modul satirinin TtlSeconds degerine gore periyodik olarak tazeler.
/// </summary>
// islevi: Her turda ayar satirini yeniden okutur, ardindan bir sonraki turun periyodunu okunan TtlSeconds degerinden kurar.
// sistemdeki gorevi: TtlSeconds kolonunu gercek davranisa baglayan tek nokta. Limiter policy'nin GetPartition metodu senkron oldugu icin tazeleme istek yolunda yapilamaz; bu isci sayesinde ayar degisikligi restart beklemeden en gec TtlSeconds kadar sonra yururluge girer.
// mirastan gelen:
//   ABP AsyncPeriodicBackgroundWorkerBase -> StartAsync/StopAsync ve DoWorkAsync hatalarinin loglanmasi (ayrica try/catch yazilmaz)
// burada yazilan: DoWorkAsync, ToPeriodMilliseconds
public class RateLimitStateWorker : AsyncPeriodicBackgroundWorkerBase
{
    // Ilk tur, satir henuz okunmadigi icin tablo varsayilan TTL'i ile planlanir.
    public RateLimitStateWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = ToPeriodMilliseconds(ModuleSettingConsts.DefaultTtlSeconds);
    }

    // Ayar satirini kendi unit of work'unde okutup snapshot'i tazeler, sonra periyodu son okunan TTL'e cevirir.
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var refresher = workerContext.ServiceProvider.GetRequiredService<RateLimitStateRefresher>();
        using var unitOfWork = unitOfWorkManager.Begin();
        await refresher.RefreshAsync();
        await unitOfWork.CompleteAsync();
        Timer.Period = ToPeriodMilliseconds(refresher.LastTtlSeconds);
    }

    // Saniye cinsinden TTL'i timer'in bekledigi milisaniye periyoduna cevirir.
    private static int ToPeriodMilliseconds(int ttlSeconds)
    {
        return (int)TimeSpan.FromSeconds(ttlSeconds).TotalMilliseconds;
    }
}
