using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Dtos.EmailTemplates;
using Volo.Abp.Application.Services;

namespace Byd.CoreExtensions.Services.EmailTemplates;

/// <summary>
/// E-posta sablonu CRUD ve aktiflestirme kullanim senaryolarini tanimlar.
/// </summary>
// islevi: EmailTemplate okuma, listeleme, olusturma, guncelleme, silme ve aktiflestirme kontratini acar.
// sistemdeki gorevi: Controller ile Application implementasyonu arasindaki public servis siniridir.
public interface IEmailTemplateAppService : IApplicationService
{
    // Kimligi verilen sablonu istegin kapsaminda getirir.
    Task<EmailTemplateDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    // Istegin kapsamindaki tum sablon surumlerini getirir.
    Task<List<EmailTemplateDto>> GetListAsync(CancellationToken cancellationToken = default);

    // Dogrulanmis girdiden istegin kapsaminda yeni aktif sablon olusturur.
    Task<EmailTemplateDto> CreateAsync(CreateEmailTemplateDto input, CancellationToken cancellationToken = default);

    // Kimligi verilen sablonun ad, konu ve icerigini gunceller.
    Task<EmailTemplateDto> UpdateAsync(Guid id, UpdateEmailTemplateDto input, CancellationToken cancellationToken = default);

    // Kimligi verilen pasif sablonu siler.
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Kimligi verilen pasif sablonu kendi olay ve dilinde aktif surum yapar.
    Task<EmailTemplateDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}
