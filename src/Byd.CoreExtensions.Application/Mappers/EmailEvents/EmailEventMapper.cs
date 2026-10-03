using System.Collections.Generic;
using Byd.CoreExtensions.Dtos.EmailEvents;
using Byd.CoreExtensions.Entities.EmailEvents;
using Riok.Mapperly.Abstractions;

namespace Byd.CoreExtensions.Mappers.EmailEvents;

/// <summary>
/// EmailEvent entity ve DTO eslemelerini tanimlar.
/// </summary>
// islevi: Okuma ciktilarini Mapperly tarafindan uretilen kodla donusturur.
// sistemdeki gorevi: EmailEvent katmanlari arasindaki alan tasimanin tek sahibidir.
[Mapper]
public partial class EmailEventMapper
{
    public partial EmailEventDto Map(EmailEvent source);
    public partial List<EmailEventDto> Map(List<EmailEvent> source);
}
