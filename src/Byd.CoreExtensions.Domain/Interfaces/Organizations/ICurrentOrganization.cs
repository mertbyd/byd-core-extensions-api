using System;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Icinde bulunulan istegin organizasyon kapsami.
/// </summary>
// islevi: Yururlukteki organizasyon kapsamini okur; Change ile blok suresince gecici kapsam acar.
// sistemdeki gorevi: "Bu istek hangi organizasyon adina calisiyor" sorusunun tek cevap noktasi. Global sorgu
// filtresi, yazma damgasi ve kapsami okuyan her tuketici ayni degeri buradan alir.
public interface ICurrentOrganization
{
    // Istekte cozulmus bir organizasyon var mi.
    bool IsAvailable { get; }

    // Kapsamdaki organizasyonun kimligi; kapsam kurulmadiysa null.
    Guid? Id { get; }

    // Kapsamdaki organizasyonun adi; kapsam ad tasimiyorsa null.
    string? Name { get; }

    // Verilen kapsami blok suresince yururluge koyar; blok bitince onceki kapsam geri doner.
    IDisposable Change(Guid? id, string? name = null);
}
