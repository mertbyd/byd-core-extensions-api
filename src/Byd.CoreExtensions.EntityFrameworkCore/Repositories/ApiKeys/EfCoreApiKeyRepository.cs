using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// ApiKey extended repository sozlesmesini EF Core DbContext'e baglar.
/// </summary>
// islevi: ApiKey icin generic, bulk ve DataTable repository yeteneklerini saglar; ayrica anahtar degerine gore tekil okumayi barindirir.
// sistemdeki gorevi: IApiKeyRepository sozlesmesinin EfExtendedRepository tabanli provider uygulamasidir.
[ExposeServices(typeof(IApiKeyRepository))]
public class EfCoreApiKeyRepository
    : EfExtendedRepository<CoreExtensionsDbContext, ApiKey, Guid>,
        IApiKeyRepository
{
    // DbContext provider'ini extended repository tabanina devreder.
    public EfCoreApiKeyRepository(
        IDbContextProvider<CoreExtensionsDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    // Anahtar degerine gore tek kaydi okur; kolon uzerinde benzersiz indeks vardir.
    public virtual Task<ApiKey?> FindByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return FindAsync(entity => entity.Key == key, cancellationToken: cancellationToken);
    }
}
