using System;
using Byd.CoreExtensions.Entities;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// ApiKey extended repository sozlesmesini EF Core DbContext'e baglar.
/// </summary>
// islevi: ApiKey icin generic, bulk ve DataTable repository yeteneklerini saglar.
// sistemdeki gorevi: IApiKeyRepository sozlesmesinin EfExtendedRepository tabanli provider uygulamasidir.
// mirastan gelen:
//   EfExtendedRepository<,,> -> GetFilteredListAsync, GetFilteredCountAsync, IsDataUsedAsync
//   EfBulkRepository<,>      -> BulkInsertManyAsync, BulkUpdateManyAsync, BulkDeleteManyAsync
//   ABP EfCoreRepository<,>  -> InsertAsync, UpdateAsync, DeleteAsync, FindAsync, GetListAsync, GetCountAsync,
//                               GetPagedListAsync, GetQueryableAsync, DeleteDirectAsync
//   ABP EfCoreRepository<,,> -> GetAsync(id), FindAsync(id), DeleteAsync(id), DeleteManyAsync(ids)
// burada yazilan: yok - anahtarlar toplu okundugu icin miras yuzeyi yetiyor.
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
}
