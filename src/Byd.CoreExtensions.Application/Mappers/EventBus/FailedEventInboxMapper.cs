using System.Collections.Generic;
using Byd.CoreExtensions.Dtos.EventBus;
using Byd.CoreExtensions.Entities.EventBus;
using Riok.Mapperly.Abstractions;

namespace Byd.CoreExtensions.Mappers.EventBus;

/// <summary>
/// Basarisiz olay entity ve DTO eslemelerini tanimlar.
/// </summary>
// islevi: Tekil ve liste repository ciktilarini Mapperly tarafindan uretilen kodla donusturur.
// sistemdeki gorevi: Basarisiz olay okuma katmanlari arasindaki alan tasimanin tek sahibidir.
[Mapper]
public partial class FailedEventInboxMapper
{
    // Tek basarisiz olay entity'sini public cevap sozlesmesine esler.
    public partial FailedEventInboxDto Map(FailedEventInbox source);

    // Basarisiz olay entity listesini public cevap listesine esler.
    public partial List<FailedEventInboxDto> Map(List<FailedEventInbox> source);
}
