using System;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.Organizations;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.Providers;

/// <summary>
/// Istegin organizasyon kapsamini okur.
/// </summary>
// islevi: Yururlukteki kapsami saklama noktasindan okur ve Change ile gecici kapsam acar.
// sistemdeki gorevi: Kapsamin tek okuma noktasi. Kapsami kimin kurdugunu bilmez; istek yolunda middleware,
// API anahtari yolunda dogrulama filtresi, arka plan isinde cagiranin kendisi kurar.
public class CurrentOrganization(ICurrentOrganizationAccessor accessor)
    : ICurrentOrganization, ITransientDependency
{
    // Istekte cozulmus bir organizasyon var mi.
    public bool IsAvailable => Id.HasValue;

    // Kapsamdaki organizasyonun kimligi; kapsam kurulmadiysa null.
    public Guid? Id => accessor.Current?.OrganizationId;

    // Kapsamdaki organizasyonun adi; kapsam ad tasimiyorsa null.
    public string? Name => accessor.Current?.Name;

    // Verilen kapsami blok suresince yururluge koyar; blok bitince onceki kapsam geri doner.
    public IDisposable Change(Guid? id, string? name = null)
    {
        var previous = accessor.Current;
        accessor.Current = new BasicOrganizationInfo(id, name);
        return new DisposeAction(() => accessor.Current = previous);
    }
}
