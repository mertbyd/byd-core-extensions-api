using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Distributed;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Olay kutularini modul DbContext'ine baglayan uzanti.
/// </summary>
// islevi: Giden ve gelen kutuyu modul DbContext'ine baglar; gelen kutu icin kayit sahibi kapsamli store'u cercevenin store'u yerine koyar.
// sistemdeki gorevi: EF Core modulunun olay kutularini tek kompozisyon noktasindan etkinlestirmesini saglar; tuketen host'ta kablolama gerekmez.
public static class EventBoxConfigurationExtensions
{
    // Kutulari modul DbContext'ine baglar ve gelen kutu store'unu kapsamli uygulamayla degistirir.
    public static IServiceCollection AddCoreExtensionsEventBoxes(this IServiceCollection services)
    {
        services.Configure<AbpDistributedEventBusOptions>(options =>
        {
            options.Outboxes.Configure(config => config.UseDbContext<CoreExtensionsDbContext>());
            options.Inboxes.Configure(config => config.UseDbContext<CoreExtensionsDbContext>());
        });

        // Acik generic store konvansiyonel kayitla yakalanmaz; cercevenin kaydi burada degistirilir.
        services.Replace(ServiceDescriptor.Transient(
            typeof(IDbContextEventInbox<>),
            typeof(CoreExtensionsEventInbox<>)));

        return services;
    }
}
