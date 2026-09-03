using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Entities.EventBus;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Basarisiz olay kutusu repository sozlesmesini EF Core DbContext'e baglar.
/// </summary>
// islevi: Kalici kayit yeteneklerine kapsamli ve zamani gelmis yeniden deneme sorgusunu ekler.
// sistemdeki gorevi: IFailedEventInboxRepository sozlesmesinin EfExtendedRepository tabanli provider uygulamasidir.
// mirastan gelen:
//   EfExtendedRepository<,,> -> GetFilteredListAsync, GetFilteredCountAsync, IsDataUsedAsync
//   EfBulkRepository<,>      -> BulkInsertManyAsync, BulkUpdateManyAsync, BulkDeleteManyAsync
//   ABP EfCoreRepository<,>  -> InsertAsync, UpdateAsync, DeleteAsync, FindAsync, GetListAsync, GetCountAsync,
//                               GetPagedListAsync, GetQueryableAsync, DeleteDirectAsync
//   ABP EfCoreRepository<,,> -> GetAsync(id), FindAsync(id), DeleteAsync(id), DeleteManyAsync(ids)
// burada yazilan: GetRetryableListAsync - kapsam ve zaman kosuluna uyan kayitlari sirali ve sinirli getirir.
[ExposeServices(typeof(IFailedEventInboxRepository))]
public class EfCoreFailedEventInboxRepository
    : EfExtendedRepository<CoreExtensionsDbContext, FailedEventInbox, Guid>,
        IFailedEventInboxRepository
{
    // DbContext provider'ini extended repository tabanina devreder.
    public EfCoreFailedEventInboxRepository(
        IDbContextProvider<CoreExtensionsDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    // Kapsami eslesen, zamani gelmis ve islenmemis kayitlari en eskiden baslayarak getirir.
    public virtual async Task<List<FailedEventInbox>> GetRetryableListAsync(
        string ownerModuleName,
        string ownerEnvironmentName,
        DateTime now,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(entity =>
                !entity.IsProcessed &&
                entity.OwnerModuleName == ownerModuleName &&
                entity.OwnerEnvironmentName == ownerEnvironmentName &&
                entity.NotBefore <= now)
            .OrderBy(entity => entity.CreationTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }
}
