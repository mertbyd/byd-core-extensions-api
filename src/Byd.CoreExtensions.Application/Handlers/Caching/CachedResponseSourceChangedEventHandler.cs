using System.Threading.Tasks;
using Byd.CoreExtensions.Etos.Caching;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.Handlers.Caching;

/// <summary>
/// Isaretli entity degisince o tipin onbellekli yanit grubunu siler.
/// </summary>
// islevi: Olayin tenant'ina gecip gruptaki tum yanitlari depodan siler.
// sistemdeki gorevi: Yanit onbelleginin veriyle ayni anda tazelenmesini saglar; olay commit tamamlaninca geldigi icin geri alinan
// bir degisiklik onbellegi bosuna silmez.
public class CachedResponseSourceChangedEventHandler(ICachedResponseStore store, ICurrentTenant currentTenant)
    : ILocalEventHandler<CachedResponseSourceChangedEto>, ITransientDependency
{
    // Olayin tenant'inda grubu temizler.
    public async Task HandleEventAsync(CachedResponseSourceChangedEto eventData)
    {
        using (currentTenant.Change(eventData.TenantId))
            await store.RemoveGroupAsync(eventData.Group);
    }
}
