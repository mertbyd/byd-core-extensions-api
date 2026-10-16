using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Attributes;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Caching;

// islevi: Her gercek calismada sayaci artiran, biri isaretli biri isaretsiz iki metotlu test servisidir.
// sistemdeki gorevi: Interceptor'un yalniz isaretli metodu sakladigini ve grup temizliginden sonra yeniden calistirdigini
// kanitlar.
public class CachedProbeAppService : ApplicationService, ICachedProbeAppService
{
    // Testin temizledigi onbellek grubu.
    public const string Group = "CachedProbe";

    private static int _callCount;

    [CachedResponse(Group)]
    public virtual Task<CachedProbeDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(new CachedProbeDto { Id = id, CallNumber = Interlocked.Increment(ref _callCount) });

    public virtual Task<CachedProbeDto> GetUncachedAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(new CachedProbeDto { Id = id, CallNumber = Interlocked.Increment(ref _callCount) });
}
