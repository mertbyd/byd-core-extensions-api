using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace Byd.CoreExtensions.Managers;

/// <summary>
/// Predicate tabanli getirme, existence ve uniqueness guard'lariyla concrete manager'lara ortak taban saglayan base manager.
/// </summary>
// islevi: TEntity uzerinde tek/coklu kayit getirme ve varlik/benzersizlik kontrollerini bizim lokalize BusinessException sozlesmemizle sunar.
// sistemdeki gorevi: Tum concrete manager'larin turedigi Foundation tabani (DomainService); sorgu detayini IExtendedRepository'ye devreder, hatalari GeneralExceptionCodes uzerinden lokalize eder.
public abstract class BaseManager<TEntity, TKey> : DomainService
    where TEntity : class, IEntity<TKey>
{
    // Guard metotlarinin uzerinde calistigi genisletilmis repository; sorgu ve toplu isler buraya devredilir.
    protected IExtendedRepository<TEntity> Repository { get; }

    protected BaseManager(IExtendedRepository<TEntity> repository)
    {
        Repository = repository;
    }
    // Predicate'e uyan tek kaydi getirir; yoksa ve throwIfNotExists ise lokalize NotFound firlatir, degilse null doner.
    public virtual async Task<TEntity?> TryGetByAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(predicate, cancellationToken: cancellationToken);
        if (entity is null && throwIfNotExists)
            throw new BusinessException(GeneralExceptionCodes.EntityNotFound);
        return entity;
    }
    // Id ile tek kaydi getirir; id esitligini predicate'e cevirip TryGetByAsync'e devreder.
    public virtual Task<TEntity?> TryGetByIdAsync(
        TKey id,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default)
    {
        return TryGetByAsync(BuildIdPredicate(id), throwIfNotExists, cancellationToken);
    }
    // Predicate'e uyan kayitlari liste olarak getirir; liste bos ve throwIfEmpty ise lokalize NotFound firlatir.
    public virtual async Task<List<TEntity>> TryGetListByAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool throwIfEmpty = false,
        CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetListAsync(predicate, cancellationToken: cancellationToken);
        if (entities.Count == 0 && throwIfEmpty)
            throw new BusinessException(GeneralExceptionCodes.EntityNotFound);
        return entities;
    }
    // Predicate'e uyan kayit yoksa lokalize NotFound firlatir; kaydi getirmeden yalnizca varligi dogrular.
    public virtual async Task CheckExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (await Repository.FindAsync(predicate, cancellationToken: cancellationToken) is null)
            throw new BusinessException(GeneralExceptionCodes.EntityNotFound);
    }
    // Predicate'e uyan kayit zaten varsa lokalize AlreadyExists firlatir; insert oncesi benzersizlik kontrolu.
    public virtual async Task CheckAlreadyExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (await Repository.FindAsync(predicate, cancellationToken: cancellationToken) is not null)
            throw new BusinessException(GeneralExceptionCodes.AlreadyExists);
    }
    // Guncelleme senaryosunda kaydin kendisini haric tutarak benzersizligi kontrol eder.
    public virtual Task CheckAlreadyExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        TKey excludeId,
        CancellationToken cancellationToken = default)
    {
        return CheckAlreadyExistsAsync(ExcludeId(predicate, excludeId), cancellationToken);
    }
    // Verilen id kumesinin tamami veritabaninda var mi diye bakar; eksik varsa lokalize NotFound firlatir.
    public virtual async Task CheckAllExistAsync(
        IEnumerable<TKey> ids,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = ids.Distinct().ToList();
        if (distinctIds.Count == 0)
            return;
        var existing = await Repository.GetListAsync(
            entity => distinctIds.Contains(entity.Id),
            cancellationToken: cancellationToken);
        if (existing.Count != distinctIds.Count)
            throw new BusinessException(GeneralExceptionCodes.EntityNotFound);
        
    }
    // Zorunlu metni bosluklardan arindirir; sonuc bos ise lokalize RequiredValueMissing firlatir.
    protected static string NormalizeRequiredText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessException(GeneralExceptionCodes.RequiredValueMissing);
        return value.Trim();
    }
    // Istege bagli metni bosluklardan arindirir; bos ise null doner.
    protected static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    // Enum degerinin tanimli bir uye olup olmadigini dogrular; degilse lokalize InvalidEnumValue firlatir.
    protected static TEnum EnsureEnumDefined<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new BusinessException(GeneralExceptionCodes.InvalidEnumValue);
        return value;
    }
    // Id esitligini (entity.Id == id) EF'in cevirebilecegi tipli bir predicate'e donusturur.
    private static Expression<Func<TEntity, bool>> BuildIdPredicate(TKey id)
    {
        var parameter = Expression.Parameter(typeof(TEntity));
        var body = Expression.Equal(
            Expression.Property(parameter, nameof(IEntity<TKey>.Id)),
            Expression.Constant(id, typeof(TKey)));
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
    // Var olan predicate'i "ve entity.Id != excludeId" kosuluyla birlestirir.
    private static Expression<Func<TEntity, bool>> ExcludeId(
        Expression<Func<TEntity, bool>> predicate,
        TKey excludeId)
    {
        var parameter = predicate.Parameters[0];
        var idNotEqual = Expression.NotEqual(
            Expression.Property(parameter, nameof(IEntity<TKey>.Id)),
            Expression.Constant(excludeId, typeof(TKey)));
        var body = Expression.AndAlso(predicate.Body, idNotEqual);

        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
}
