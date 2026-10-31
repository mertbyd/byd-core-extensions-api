using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Etos.Caching;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus.Local;
namespace Byd.CoreExtensions.EntityFrameworkCore;
/// <summary>
/// Aktiflik ve organizasyon kapsami filtrelerini uygulayan DbContext temeli.
/// </summary>
// islevi: IHasIsActive ve IMultiOrganization tasiyan entity'ler icin EF Core global query filter kurar, yeni
// kayitlara organizasyon kapsamini damgalar, ICachedResponseSource tasiyan entity degisince o tipin yanit grubunun
// temizlenmesini commit sonrasina yazar.
// sistemdeki gorevi: Tuketici modullerin turedigi ortak DbContext tabani. Iki filtreyi de tek yerde tutar ve
// ABP'nin soft-delete/multi-tenant filtreleriyle birlesik calisir.
public abstract class BydDbContext<TDbContext> : AbpDbContext<TDbContext>
    where TDbContext : DbContext
{
    // Aktiflik filtresinin o anki durumunu ABP veri filtresi altyapisindan okur.
    protected virtual bool IsIsActiveFilterEnabled
        => DataFilter?.IsEnabled<IHasIsActive>() ?? false;
    // Organizasyon filtresinin o anki durumunu ABP veri filtresi altyapisindan okur. Tenant filtresinden ayri
    // bir bayraktir; birini kapatmak digerini kapatmaya zorlamaz.
    protected virtual bool IsOrganizationFilterEnabled
        => DataFilter?.IsEnabled<IMultiOrganization>() ?? false;
    // Yururlukteki organizasyon kimligi. Okuma filtresi ve yazma damgasi ayni kaynaktan okur; iki yol ayrilsaydi
    // hicbir sorgunun bulamayacagi satirlar yazilir ve bu sessizce olurdu.
    protected virtual Guid? CurrentOrganizationId
        => LazyServiceProvider?.LazyGetRequiredService<ICurrentOrganization>().Id;
    protected BydDbContext(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }
    // Kaydetmeden once degisen isaretli tipleri toplar; kayit basarili olursa her tip icin grup temizligini commit sonrasina
    // yazar. Toplama kaydetmeden once yapilir cunku kaydettikten sonra izleyicideki durumlar sifirlanir.
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var changedGroups = CollectChangedCachedResponseGroups();
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        await PublishCachedResponseGroupsChangedAsync(changedGroups, cancellationToken);
        return result;
    }
    // Eklenen, degisen ya da silinen isaretli entity'lerin tip adlarini tekil olarak dondurur.
    protected virtual List<string> CollectChangedCachedResponseGroups()
    {
        return ChangeTracker.Entries<ICachedResponseSource>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => entry.Metadata.ClrType.Name)
            .Distinct()
            .ToList();
    }
    // Her grup icin temizlik olayini yururlukteki tenant'la commit tamamlaninca yayinlanacak sekilde kuyruga koyar.
    protected virtual async Task PublishCachedResponseGroupsChangedAsync(
        List<string> groups,
        CancellationToken cancellationToken = default)
    {
        if (groups.Count == 0)
            return;
        var localEventBus = LazyServiceProvider.LazyGetRequiredService<ILocalEventBus>();
        foreach (var group in groups)
            await localEventBus.PublishAsync(new CachedResponseSourceChangedEto(group, CurrentTenant?.Id));
    }
    // Entity iki isaretten birini tasiyorsa global filtreye dahil eder, aksi halde ABP'nin kararini korur.
    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(IHasIsActive).IsAssignableFrom(typeof(TEntity))
            || typeof(IMultiOrganization).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }
        return base.ShouldFilterEntity<TEntity>(entityType);
    }
    // ABP'nin mevcut filtre ifadesine tasinan isaretlerin kosullarini ekler; bayraklar ifadenin icinde tutuldugu
    // icin filtreler calisma aninda acilip kapanabilir.
    protected override Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>(
        ModelBuilder modelBuilder,
        EntityTypeBuilder<TEntity> entityTypeBuilder)
    {
        var expression = base.CreateFilterExpression<TEntity>(modelBuilder, entityTypeBuilder);
        if (typeof(IHasIsActive).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> isActiveFilter =
                entity => !IsIsActiveFilterEnabled
                          || EF.Property<bool>(entity, nameof(IHasIsActive.IsActive));
            expression = CombineFilters(expression, isActiveFilter);
        }
        if (typeof(IMultiOrganization).IsAssignableFrom(typeof(TEntity)))
        {
            // Kapsam yoksa yalnizca kapsamsiz satirlar gorunur; ABP'nin tenant filtresi de ayni anlami tasir.
            Expression<Func<TEntity, bool>> organizationFilter =
                entity => !IsOrganizationFilterEnabled
                          || EF.Property<Guid>(entity, nameof(IMultiOrganization.OrganizationId)) == CurrentOrganizationId;
            expression = CombineFilters(expression, organizationFilter);
        }
        return expression;
    }
    // Yeni kayda yururlukteki organizasyon kapsamini damgalar. Deger cagirandan alinmaz: kapsam kolonunu istege
    // birakmak, yazan tarafin okuyan tarafla ayni kapsami kullandigini dogrulanamaz hale getirir.
    protected override void ApplyAbpConceptsForAddedEntity(EntityEntry entry)
    {
        base.ApplyAbpConceptsForAddedEntity(entry);
        if (entry.Entity is IMultiOrganization)
        {
            entry.Property(nameof(IMultiOrganization.OrganizationId)).CurrentValue = CurrentOrganizationId;
        }
    }
    // Iki filtre ifadesini tek ifadede birlestirir; onceki ifade yoksa yenisi oldugu gibi doner.
    private static Expression<Func<TEntity, bool>> CombineFilters<TEntity>(
        Expression<Func<TEntity, bool>>? current,
        Expression<Func<TEntity, bool>> addition)
        where TEntity : class
        => current == null
            ? addition
            : QueryFilterExpressionHelper.CombineExpressions(current, addition);
}
