using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EventBus.Distributed;
namespace Byd.CoreExtensions.EventBus;
/// <summary>
/// Yapilandirilmis her gelen olay kutusu icin bir basarisiz olay isleyicisi acar ve yasam dongusunu yonetir.
/// </summary>
// islevi: Acilista islemeye acik her InboxConfig icin ayri isleyici cozer, baslatir ve kapanista hepsini durdurur.
// sistemdeki gorevi: Isleyicilerin sayisini ve yasam suresini yapilandirmaya baglayan tekil arka plan iscisidir.
// Adi bilerek Worker'dir: bu tip bir domain manager degil, cercevenin arka plan isci sozlesmesini uygulayan
// bir altyapi bilesenidir ve Application katmaninda yasar.
public class FailedEventInboxProcessWorker : IBackgroundWorker
{
    private readonly AbpDistributedEventBusOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IFailedEventInboxProcessor> _processors;

    // Yapilandirma sozlugunu ve isleyici cozumleme kaynagini alir.
    public FailedEventInboxProcessWorker(
        IOptions<AbpDistributedEventBusOptions> options,
        IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _processors = new List<IFailedEventInboxProcessor>();
    }
    // Islemeye acik her yapilandirma icin bir isleyici acar ve baslatir.
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        foreach (var inboxConfiguration in _options.Inboxes.Values)
        {
            if (!inboxConfiguration.IsProcessingEnabled)
                continue;
            var processor = _serviceProvider.GetRequiredService<IFailedEventInboxProcessor>();
            await processor.StartAsync(inboxConfiguration, cancellationToken);
            _processors.Add(processor);
        }
    }
    // Acilmis butun isleyicileri durdurur.
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var processor in _processors)
        {
            await processor.StopAsync(cancellationToken);
        }
    }
}
