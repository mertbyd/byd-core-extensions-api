using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Dtos.EmailEvents;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Mappers.EmailEvents;

namespace Byd.CoreExtensions.Services.EmailEvents;

/// <summary>
/// E-posta olaylarinin okuma orkestrasyonunu yurutur.
/// </summary>
// islevi: Olay kayitlarini Manager guard'i ve repository uzerinden okuyup DTO'ya cevirir.
// sistemdeki gorevi: Controller ile EmailEvent Manager/Repository zinciri arasindaki tek public giris noktasidir.
public class EmailEventAppService : CoreExtensionsAppService, IEmailEventAppService
{
    private static readonly EmailEventMapper Mapper = new();

    private EmailEventManager Manager =>
        LazyGetRequiredService<EmailEventManager>();

    private IEmailEventRepository Repository =>
        LazyGetRequiredService<IEmailEventRepository>();
    // Kimligi verilen olayi Manager guard'i uzerinden getirir.
    public virtual async Task<EmailEventDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken);
        return Mapper.Map(entity);
    }
    // Seed edilmis tum olaylari repository'den getirir.
    public virtual async Task<List<EmailEventDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetListAsync(
            includeDetails: false,
            cancellationToken: cancellationToken);
        return Mapper.Map(entities);
    }
}
