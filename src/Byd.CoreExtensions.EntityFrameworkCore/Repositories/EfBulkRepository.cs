using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.Repositories;

/// <summary>
/// Toplu yazma islemlerini saglayiciya ozgu kume tabanli yolla yapan repository tabani.
/// </summary>
// islevi: BulkInsert/BulkUpdate/BulkDelete cagrilarini EFCore.BulkExtensions uzerinden, degisiklik takipcisini atlayarak calistirir.
// sistemdeki gorevi: IBulkRepository sozlesmesinin EF Core uygulamasi; toplu yazma gereken repository'ler ABP'nin EfCoreRepository'si yerine bundan turer.
public abstract class EfBulkRepository<TDbContext, TEntity>
    : EfCoreRepository<TDbContext, TEntity>, IBulkRepository<TEntity>
    where TDbContext : DbContext, IEfCoreDbContext
    where TEntity : class, IEntity
{
    protected EfBulkRepository(IDbContextProvider<TDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
    // Kayitlari tek kume islemiyle ekler.
    public virtual async Task BulkInsertManyAsync(
        IEnumerable<TEntity> entities,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.BulkInsertAsync(
            entities.ToList(),
            CreateBulkConfig(batchSize),
            cancellationToken: cancellationToken);
    }
    // Kayitlari tek kume islemiyle gunceller.
    public virtual async Task BulkUpdateManyAsync(
        IEnumerable<TEntity> entities,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.BulkUpdateAsync(
            entities.ToList(),
            CreateBulkConfig(batchSize),
            cancellationToken: cancellationToken);
    }
    // Kayitlari tek kume islemiyle siler; kalici silme yapar, ABP'nin soft-delete kesicisini calistirmaz.
    public virtual async Task BulkDeleteManyAsync(
        IEnumerable<TEntity> entities,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        TDbContext dbContext = await GetDbContextAsync();
        await dbContext.BulkDeleteAsync(
            entities.ToList(),
            CreateBulkConfig(batchSize),
            cancellationToken: cancellationToken);
    }
    // Cagirici batch boyutu vermediyse kararli varsayilani uygular; turetilen repository gerekirse ezebilir.
    protected virtual BulkConfig CreateBulkConfig(int? batchSize)
    {
        return new BulkConfig
        {
            BatchSize = batchSize ?? BulkOperationConsts.DefaultBatchSize
        };
    }
}
