using Byd.CoreExtensions.ApiKeys;
using Byd.CoreExtensions.Dtos.ApiKeys;
using Byd.CoreExtensions.Entities;
using Byd.CoreExtensions.Models.ApiKeys;
using Riok.Mapperly.Abstractions;

namespace Byd.CoreExtensions.Mappers.ApiKeys;

/// <summary>
/// ApiKey DTO, domain model, entity ve bellek goruntusu eslemelerini tanimlar.
/// </summary>
// islevi: Uretim girdisini, okuma cikisini ve dogrulama goruntusunu Mapperly tarafindan uretilen kodla donusturur.
// sistemdeki gorevi: ApiKey katmanlari arasindaki alan tasimanin tek sahibidir.
[Mapper]
public partial class ApiKeyMapper
{
    public partial ApiKeyCreateModel Map(CreateApiKeyDto source);
    public partial ApiKeyDto Map(ApiKey source);
    public partial CreatedApiKeyDto MapCreated(ApiKey source);
    public partial ApiKeyDescriptor MapDescriptor(ApiKey source);
}
