using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Extensions;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Querying;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.Repositories;

/// <summary>
/// Toplu yazma yeteneklerine ek olarak DataTable filtreleme ve iliski kontrolu saglayan repository tabani.
/// </summary>
// islevi: DataTable istegindeki filtre/siralama/sayfalamayi DataTableQueryBuilder ile sorguya uygular ve silme oncesi iliski kontrolu yapar.
// sistemdeki gorevi: Repository zincirinin orta halkasinin EF Core uygulamasi; EfBulkRepository'den turer, ICachedRepository de bunu genisletir.
public abstract class EfExtendedRepository<TDbContext, TEntity, TKey>
    : EfBulkRepository<TDbContext, TEntity>, IExtendedRepository<TEntity>
    where TDbContext : DbContext, IEfCoreDbContext
    where TEntity : class, IEntity<TKey>
{
    // Bagimli entity tipi ancak calisma aninda bilindigi icin sorgu jenerik bir yardimciya tasindi;
    // yansima yalnizca bu tek metodu somutlastirmak icin kullanilir.
    private static readonly MethodInfo AnyReferencingMethod =
        typeof(EfExtendedRepository<TDbContext, TEntity, TKey>)
            .GetMethod(nameof(AnyReferencingAsync), BindingFlags.NonPublic | BindingFlags.Static)!;
    protected EfExtendedRepository(IDbContextProvider<TDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    // Filtre, siralama ve sayfalamayi uygulayip sayfa kayitlarini dondurur.
    public virtual async Task<List<TEntity>> GetFilteredListAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        CancellationToken cancellationToken)
        where TFiltering : class
    {
        var query = await CreateFilteredQueryAsync(pagingProperty);
        query = query.ApplySorting<TEntity, TKey>(pagingProperty.Sorting, typeof(TFiltering));
        query = query.ApplyPaging(pagingProperty.Page, pagingProperty.PageSize);
        return await query.ToListAsync(cancellationToken);
    }
    // Filtreleri uygulayip sayfalamadan onceki toplam kayit sayisini dondurur.
    public virtual async Task<int> GetFilteredCountAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty,
        CancellationToken cancellationToken)
        where TFiltering : class
    {
        var query = await CreateFilteredQueryAsync(pagingProperty);
        return await query.CountAsync(cancellationToken);
    }
    // Kaydi isaret eden yabanci anahtarlari sirayla tarar, ilk eslesmede kaydin kullanildigini bildirir.
    public virtual async Task<bool> IsDataUsedAsync(
        TEntity entity,
        IEnumerable<Type>? ignoredEntityTypes,
        CancellationToken cancellationToken)
    {
        var dbContext = await GetDbContextAsync();
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        if (entityType?.FindPrimaryKey() == null)
        {
            return false;
        }
        var keyValues = ReadPrimaryKeyValues(dbContext, entity, entityType);
        var ignored = ignoredEntityTypes?.ToHashSet() ?? new HashSet<Type>();
        foreach (var foreignKey in FindCheckableForeignKeys(entityType, ignored))
        {
            if (await HasReferencingRowAsync(dbContext, foreignKey, keyValues, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    // Filtreleri uygulanmis, takip edilmeyen sorguyu hazirlar; siralama ve sayfalama cagiran metotta eklenir.
    protected virtual async Task<IQueryable<TEntity>> CreateFilteredQueryAsync<TFiltering>(
        IDataTablePagingProperty<TFiltering> pagingProperty)
        where TFiltering : class
    {
        var query = (await GetQueryableAsync()).AsNoTracking();

        return query.ApplyFilters(pagingProperty.Filters);
    }

    // Kaydin birincil anahtar degerlerini property adiyla eslestirir.
    private static IReadOnlyDictionary<string, object?> ReadPrimaryKeyValues(
        DbContext dbContext,
        TEntity entity,
        IEntityType entityType)
    {
        var entry = dbContext.Entry(entity);

        return entityType.FindPrimaryKey()!.Properties.ToDictionary(
            property => property.Name,
            property => entry.Property(property.Name).CurrentValue);
    }

    // Entity'yi isaret eden yabanci anahtarlardan gormezden gelinmeyenleri secer.
    private static IEnumerable<IForeignKey> FindCheckableForeignKeys(
        IEntityType entityType,
        ICollection<Type> ignoredEntityTypes)
    {
        return entityType
            .GetReferencingForeignKeys()
            .Where(foreignKey => !ignoredEntityTypes.Contains(foreignKey.DeclaringEntityType.ClrType));
    }
    // Tek bir yabanci anahtar icin bagimli tabloda eslesen satir olup olmadigini sorgular.
    private static async Task<bool> HasReferencingRowAsync(
        DbContext dbContext,
        IForeignKey foreignKey,
        IReadOnlyDictionary<string, object?> principalKeyValues,
        CancellationToken cancellationToken)
    {
        var dependentClrType = foreignKey.DeclaringEntityType.ClrType;
        var parameter = Expression.Parameter(dependentClrType, DataTableQueryConsts.DependentParameterName);
        var predicate = BuildForeignKeyPredicate(foreignKey, principalKeyValues, parameter);
        if (predicate == null)
        {
            return false;
        }
        var lambda = Expression.Lambda(predicate, parameter);
        var anyTask = (Task<bool>)AnyReferencingMethod
            .MakeGenericMethod(dependentClrType)
            .Invoke(null, new object[] { dbContext, lambda, cancellationToken })!;
        return await anyTask;
    }
    // Bagimli tabloda kosulu saglayan satir var mi diye bakar; TDependent burada derleme zamani tiptir.
    private static Task<bool> AnyReferencingAsync<TDependent>(
        DbContext dbContext,
        Expression<Func<TDependent, bool>> predicate,
        CancellationToken cancellationToken)
        where TDependent : class
    {
        return dbContext.Set<TDependent>().AnyAsync(predicate, cancellationToken);
    }
    // Yabanci anahtarin tum sutunlarini asil anahtar degerleriyle esleyen kosulu kurar.
    private static Expression? BuildForeignKeyPredicate(
        IForeignKey foreignKey,
        IReadOnlyDictionary<string, object?> principalKeyValues,
        ParameterExpression parameter)
    {
        Expression? predicate = null;
        for (var index = 0; index < foreignKey.Properties.Count; index++)
        {
            var keyValue = ResolveKeyValue(foreignKey, principalKeyValues, index);
            // Anahtar degeri okunamiyorsa eslesme kurulamaz; bu yabanci anahtar tamamen atlanir.
            if (keyValue == null)
            {
                return null;
            }
            var comparison = BuildColumnComparison(parameter, foreignKey.Properties[index], keyValue);
            predicate = predicate == null ? comparison : Expression.AndAlso(predicate, comparison);
        }
        return predicate;
    }
    // Bagimli sutunun asil anahtardaki karsiligini okur.
    private static object? ResolveKeyValue(
        IForeignKey foreignKey,
        IReadOnlyDictionary<string, object?> principalKeyValues,
        int index)
    {
        var principalProperty = foreignKey.PrincipalKey.Properties[index];
        return principalKeyValues.TryGetValue(principalProperty.Name, out var keyValue) ? keyValue : null;
    }
    // Tek bir sutun icin esitlik kosulunu kurar.
    private static Expression BuildColumnComparison(
        ParameterExpression parameter,
        IProperty dependentProperty,
        object keyValue)
    {
        var propertyAccess = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { dependentProperty.ClrType },
            parameter,
            Expression.Constant(dependentProperty.Name));
        return Expression.Equal(propertyAccess, Expression.Constant(keyValue, dependentProperty.ClrType));
    }
}
