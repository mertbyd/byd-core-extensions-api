using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Attributes;

namespace Byd.CoreExtensions.Caching;

// islevi: Biri isaretli, biri isaretsiz, biri isaretli metodu icinden cagiran uc metotlu test servisidir.
// sistemdeki gorevi: Interceptor'un yalniz proxy'den gelen isaretli cagriyi sakladigini ve grup temizliginden sonra yeniden
// calistirdigini kanitlar.
public class CachedProbeAppService : CachedProbeAppServiceBase, ICachedProbeAppService
{
    [CachedResponse(Group)]
    public virtual Task<CachedProbeDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Next(id));

    public virtual Task<CachedProbeDto> GetUncachedAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Next(id));

    public virtual Task<CachedProbeDto> GetThroughInnerCallAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync(id, cancellationToken);
}
