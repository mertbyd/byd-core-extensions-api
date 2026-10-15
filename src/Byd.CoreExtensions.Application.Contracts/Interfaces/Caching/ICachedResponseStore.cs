using System.Threading;
using System.Threading.Tasks;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Onbellege alinan API yanitlarini grup grup temizleyen depo sozlesmesi.
/// </summary>
// islevi: Yururlukteki tenant'in verilen gruptaki tum onbellekli yanitlarini siler.
// sistemdeki gorevi: Isaretli entity degisikligi gruplari kendiliginden temizler; bir modul veriyi entity disi bir yoldan
// degistirdiginde (toplu ice aktarma, aktif kayit degisimi) ayni temizligi bu sozlesmeyle elle ister.
public interface ICachedResponseStore
{
    // Yururlukteki tenant'in gruptaki tum yanitlarini siler; Redis kapaliysa islem bostur.
    Task RemoveGroupAsync(string group, CancellationToken cancellationToken = default);
}
