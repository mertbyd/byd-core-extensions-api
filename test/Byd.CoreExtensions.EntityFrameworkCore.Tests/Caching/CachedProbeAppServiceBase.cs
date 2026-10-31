using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Attributes;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Caching;

// islevi: Isaretli metodu taban sinifta tasiyan test servis tabanidir; her gercek calismada ortak sayaci artirir.
// sistemdeki gorevi: Tuketici modullerin CRUD tabanindaki isaretin turetilmis serviste de calistigini kanitlar.
public abstract class CachedProbeAppServiceBase : ApplicationService
{
    // Testin temizledigi onbellek grubu.
    public const string Group = "CachedProbe";

    private static int _callCount;

    [CachedResponse(Group)]
    public virtual Task<CachedProbeDto> GetFromBaseAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Next(id));

    // Sayaci artirip yeni sonucu uretir.
    protected static CachedProbeDto Next(Guid id) => new() { Id = id, CallNumber = Interlocked.Increment(ref _callCount) };
}
