using System;

namespace Byd.CoreExtensions.Dtos.EmailEvents;

/// <summary>
/// Kalici e-posta olayi kaydinin public cevap alanlarini tasir.
/// </summary>
// islevi: Olay kimligini, lookup alanlarini ve olusturma zamanini HTTP ciktisina acar.
// sistemdeki gorevi: Entity veya EF tiplerini disariya sizdirmayan response sozlesmesidir.
public class EmailEventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int? Code { get; set; }
    public DateTime CreationTime { get; set; }
}
