using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Dtos.EmailEvents;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Services.EmailEvents;

/// <summary>
/// E-posta olayi okuma kullanim senaryolarini tanimlar.
/// </summary>
// islevi: EmailEvent okuma ve listeleme kontratini acar; olaylari onlari gonderen moduller seed eder.
// sistemdeki gorevi: Controller ile Application implementasyonu arasindaki public servis siniridir.
public interface IEmailEventAppService : IApplicationService
{
    // Kimligi verilen olayi getirir.
    Task<EmailEventDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    // Seed edilmis tum olaylari getirir.
    Task<List<EmailEventDto>> GetListAsync(CancellationToken cancellationToken = default);
}
