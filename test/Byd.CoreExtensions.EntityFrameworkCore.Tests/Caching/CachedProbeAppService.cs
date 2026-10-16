using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Attributes;

namespace Byd.CoreExtensions.Caching;

// islevi: Biri isaretli, biri isaretsiz, biri isaretli metodu icinden cagiran okuma metotlari ile grubu temizleyen biri basarili,
// biri hatali iki yazma metodu tasiyan test servisidir.
// sistemdeki gorevi: Interceptor'un yalniz proxy'den gelen isaretli cagriyi sakladigini, grup temizliginden sonra yeniden
// calistirdigini ve hata veren yazmanin onbellegi silmedigini kanitlar.
public class CachedProbeAppService : CachedProbeAppServiceBase, ICachedProbeAppService
{
    [CachedResponse(Group)]
    public virtual Task<CachedProbeDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Next(id));

    public virtual Task<CachedProbeDto> GetUncachedAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Next(id));

    public virtual Task<CachedProbeDto> GetThroughInnerCallAsync(Guid id, CancellationToken cancellationToken = default)
        => GetAsync(id, cancellationToken);

    [ClearCachedResponse(Group)]
    public virtual Task ChangeAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    [ClearCachedResponse(Group)]
    public virtual Task ChangeAndFailAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException();
}
