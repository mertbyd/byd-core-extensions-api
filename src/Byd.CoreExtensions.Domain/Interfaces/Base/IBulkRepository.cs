using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Buyuk kayit kumelerini toplu olarak yazan repository sozlesmesi.
/// </summary>
// islevi: Coklu kayit ekleme, guncelleme ve silme islemlerini degisiklik takipcisini kullanmadan, kume tabanli tek islemde yapmayi taahhut eder.
// sistemdeki gorevi: ABP'nin satir bazli InsertMany/UpdateMany/DeleteMany metotlarinin olcek yetersiz kaldigi senaryolarin sozlesmesi; uygulama saglayiciya ozgu toplu yazma yolunu kullanir.
public interface IBulkRepository<TEntity> : IRepository<TEntity>, IScopedDependency
    where TEntity : class, IEntity
{
    // Verilen kayitlari toplu olarak ekler; batchSize bos birakilirsa BulkOperationConsts.DefaultBatchSize kullanilir.
    Task BulkInsertManyAsync(
        IEnumerable<TEntity> entities,
        int? batchSize,
        CancellationToken cancellationToken = default);
    // Verilen kayitlari toplu olarak gunceller; batchSize bos birakilirsa BulkOperationConsts.DefaultBatchSize kullanilir.
    Task BulkUpdateManyAsync(
        IEnumerable<TEntity> entities,
        int? batchSize,
        CancellationToken cancellationToken = default);
    // Verilen kayitlari toplu olarak siler; batchSize bos birakilirsa BulkOperationConsts.DefaultBatchSize kullanilir.
    Task BulkDeleteManyAsync(
        IEnumerable<TEntity> entities,
        int? batchSize,
        CancellationToken cancellationToken = default);
}
