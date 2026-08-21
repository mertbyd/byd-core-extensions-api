using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.EntityFrameworkCore;
namespace Byd.CoreExtensions.EntityFrameworkCore;
/// <summary>
/// IHasIsActive uygulayan entity'lere global aktiflik filtresi ekleyen DbContext temeli.
/// </summary>
// islevi: IHasIsActive tasiyan her entity icin EF Core global query filter kurar; filtre acikken pasif kayitlari sorgu sonucundan duser.
// sistemdeki gorevi: Tuketici modullerin turedigi ortak DbContext tabani; aktiflik filtresini tek yerde tutar ve ABP'nin soft-delete/multi-tenant filtreleriyle birlestirerek calisir.
public abstract class BydDbContext<TDbContext> : AbpDbContext<TDbContext>
    where TDbContext : DbContext
{
    // Aktiflik filtresinin o anki durumunu ABP veri filtresi altyapisindan okur.
    protected virtual bool IsIsActiveFilterEnabled
        => DataFilter?.IsEnabled<IHasIsActive>() ?? false;
    protected BydDbContext(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }
    // Entity IHasIsActive tasiyorsa global filtreye dahil eder, aksi halde ABP'nin kararini korur.
    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(IHasIsActive).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }
        return base.ShouldFilterEntity<TEntity>(entityType);
    }
    // ABP'nin mevcut filtre ifadesine aktiflik kosulunu ekler; bayrak ifadenin icinde tutuldugu icin filtre calisma aninda acilip kapanabilir.
    protected override Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>(
        ModelBuilder modelBuilder,
        EntityTypeBuilder<TEntity> entityTypeBuilder)
    {
        var expression = base.CreateFilterExpression<TEntity>(modelBuilder, entityTypeBuilder);
        if (!typeof(IHasIsActive).IsAssignableFrom(typeof(TEntity)))
        {
            return expression;
        }
        Expression<Func<TEntity, bool>> isActiveFilter =
            entity => !IsIsActiveFilterEnabled
                      || EF.Property<bool>(entity, nameof(IHasIsActive.IsActive));
        return expression == null
            ? isActiveFilter
            : QueryFilterExpressionHelper.CombineExpressions(expression, isActiveFilter);
    }
}
