using System.Collections.Generic;
using Byd.CoreExtensions.Dtos.EmailTemplates;
using Byd.CoreExtensions.Entities.EmailTemplates;
using Byd.CoreExtensions.Models.EmailTemplates;
using Riok.Mapperly.Abstractions;

namespace Byd.CoreExtensions.Mappers.EmailTemplates;

/// <summary>
/// EmailTemplate DTO, domain model ve entity eslemelerini tanimlar.
/// </summary>
// islevi: CRUD girdileri ile ciktilari Mapperly tarafindan uretilen kodla donusturur.
// sistemdeki gorevi: EmailTemplate katmanlari arasindaki alan tasimanin tek sahibidir.
[Mapper]
public partial class EmailTemplateMapper
{
    public partial EmailTemplateCreateModel Map(CreateEmailTemplateDto source);
    public partial EmailTemplateUpdateModel Map(UpdateEmailTemplateDto source);
    public partial EmailTemplateDto Map(EmailTemplate source);
    public partial List<EmailTemplateDto> Map(List<EmailTemplate> source);
}
