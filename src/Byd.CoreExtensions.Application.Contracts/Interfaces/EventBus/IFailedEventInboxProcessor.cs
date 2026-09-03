using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Tek bir gelen olay yapilandirmasina bagli basarisiz olay yeniden deneme dongusunu tanimlar.
/// </summary>
// islevi: Bir InboxConfig icin dongunun baslatilmasini ve durdurulmasini sozlesmeye baglar.
// sistemdeki gorevi: Surec yoneticisinin yapilandirma basina bagimsiz isleyici cozmesini saglayan tek soyutlamadir.
public interface IFailedEventInboxProcessor
{
    // Isleyiciyi verilen gelen olay yapilandirmasina baglar ve tur dongusunu baslatir.
    Task StartAsync(InboxConfig inboxConfiguration, CancellationToken cancellationToken = default);

    // Devam eden turu iptal eder ve tur dongusunu durdurur.
    Task StopAsync(CancellationToken cancellationToken = default);
}
