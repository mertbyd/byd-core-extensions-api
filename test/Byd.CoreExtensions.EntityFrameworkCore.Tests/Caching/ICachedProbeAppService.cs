using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Caching;

// islevi: Yanit onbellegi interceptor'unu arayuz uzerinden cagirmak icin test servisinin sozlesmesidir.
// sistemdeki gorevi: Gercek tuketicideki gibi cagrinin proxy'den gecmesini saglar.
public interface ICachedProbeAppService : IApplicationService
{
    Task<CachedProbeDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CachedProbeDto> GetUncachedAsync(Guid id, CancellationToken cancellationToken = default);
}
