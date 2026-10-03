using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities.EmailTemplates;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.EmailTemplates;
using Scriban;
using Scriban.Runtime;
using Volo.Abp;
using Volo.Abp.Emailing;

namespace Byd.CoreExtensions.Managers;

/// <summary>
/// E-posta sablonu olusturma, aktiflik, kapsam ve gonderim kurallarini yonetir.
/// </summary>
// islevi: Sablonu istegin kapsamina damgalar, ayni olay ve dil icin kapsamda tek aktif surum birakir, gonderimde en yakin
// kapsamin aktif sablonunu secip modelle doldurur.
// sistemdeki gorevi: AppService'i ve sablonu kullanan modulleri is kararindan, entity'yi davranistan uzak tutan domain
// sahibidir. Moduller e-postayi olay degeriyle bu Manager uzerinden gonderir.
public class EmailTemplateManager : BaseManager<EmailTemplate, Guid>
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;

    private ICurrentOrganization CurrentOrganization =>
        LazyServiceProvider.LazyGetRequiredService<ICurrentOrganization>();

    private IEmailSender EmailSender =>
        LazyServiceProvider.LazyGetRequiredService<IEmailSender>();

    private EmailEventManager EmailEventManager =>
        LazyServiceProvider.LazyGetRequiredService<EmailEventManager>();

    // EmailTemplate extended repository'sini ortak guard tabanina devreder; kapsam sorgusu icin ayrica tutar.
    public EmailTemplateManager(IEmailTemplateRepository repository)
        : base(repository)
    {
        _emailTemplateRepository = repository;
    }

    // Kimligi verilen sablonu istegin kapsaminda getirir; baska kapsamin satiri bulunamadi olarak doner.
    public virtual async Task<EmailTemplate> EnsureExistsInScopeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenant.Id;
        var organizationId = CurrentOrganization.Id;
        return (await TryGetByAsync(
            template => template.Id == id &&
                        template.TenantId == tenantId &&
                        template.OrganizationId == organizationId,
            throwIfNotExists: true,
            cancellationToken))!;
    }

    // Istegin kapsamindaki tum sablon surumlerini olay adlariyla getirir; ust kapsamlarin satirlari listelenmez.
    public virtual Task<List<EmailTemplate>> GetListInScopeAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenant.Id;
        var organizationId = CurrentOrganization.Id;
        return Repository.GetListAsync(
            template => template.TenantId == tenantId && template.OrganizationId == organizationId,
            includeDetails: true,
            cancellationToken);
    }

    // Create modelini istegin kapsamina damgali aktif sablona donusturur; kapsamdaki onceki aktif surumu pasife alir.
    // Pasife alinan satir ayni unit of work'te izlendigi icin yeni satirla birlikte kaydedilir.
    public virtual async Task<EmailTemplate> CreateAsync(
        EmailTemplateCreateModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var emailEvent = await EmailEventManager.EnsureExistsAsync(model.EmailEventId, cancellationToken);
        var entity = new EmailTemplate(GuidGenerator.Create())
        {
            TenantId = CurrentTenant.Id,
            OrganizationId = CurrentOrganization.Id,
            EmailEventId = emailEvent.Id,
            EmailEvent = emailEvent,
            CultureName = NormalizeRequiredText(model.CultureName),
            IsActive = true
        };
        Apply(entity, model);
        await DeactivateCurrentAsync(entity, cancellationToken);
        return entity;
    }

    // Sablonun ad, konu ve icerigini degistirir; olay, dil ve kapsam olusturmada sabitlenir.
    public virtual EmailTemplate Update(EmailTemplate entity, EmailTemplateUpdateModel model)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);
        Apply(entity, model);
        return entity;
    }

    // Aktif sablonun silinmesini engeller; kapsam bu olay ve dil icin sablonsuz kalmaz.
    public virtual void EnsureCanDelete(EmailTemplate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.IsActive)
            throw new BusinessException(EmailTemplateExceptionCodes.ActiveTemplateCannotBeDeleted);
    }

    // Pasif sablonu aktif yapar ve ayni olay, dil ve kapsamdaki onceki aktif surumu pasife alir.
    public virtual async Task ActivateAsync(
        EmailTemplate entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.IsActive)
            throw new BusinessException(EmailTemplateExceptionCodes.TemplateAlreadyActive);
        await DeactivateCurrentAsync(entity, cancellationToken);
        entity.IsActive = true;
    }

    // Adi verilen olayin sablonunu istegin kapsamina ve diline gore secer, konu ile icerigi modelle doldurup gonderim
    // kuyruguna birakir. Olay adi olayi gonderen modulun sabitidir.
    public virtual async Task SendAsync(
        string emailEventName,
        string to,
        object model,
        CancellationToken cancellationToken = default)
    {
        var template = await FindForCurrentScopeAsync(emailEventName, cancellationToken);
        var subject = await RenderAsync(template.Subject, model, cancellationToken);
        var body = await RenderAsync(template.Body, model, cancellationToken);
        await EmailSender.QueueAsync(to, subject, body);
    }

    // Olayin istek dilindeki aktif sablonunu organizasyon, ust organizasyon, tenant ve host sirasiyla arar; olay seed
    // edilmediyse de sablon bulunamadi olarak reddedilir.
    private async Task<EmailTemplate> FindForCurrentScopeAsync(
        string emailEventName,
        CancellationToken cancellationToken = default)
    {
        var emailEvent = await EmailEventManager.FindByNameAsync(emailEventName, cancellationToken)
                         ?? throw new BusinessException(EmailTemplateExceptionCodes.TemplateNotFound);
        var template = await _emailTemplateRepository.FindActiveForScopeAsync(
            emailEvent.Id,
            CultureInfo.CurrentUICulture.Name,
            CurrentTenant.Id,
            CurrentOrganization.Id,
            CurrentOrganization.ParentId,
            cancellationToken);
        return template ?? throw new BusinessException(EmailTemplateExceptionCodes.TemplateNotFound);
    }

    // Ayni olay, dil ve kapsamdaki aktif surumu pasife alir; verilen sablon kapsamin tek aktif satiri olur.
    private async Task DeactivateCurrentAsync(
        EmailTemplate entity,
        CancellationToken cancellationToken = default)
    {
        var current = await TryGetByAsync(
            template => template.Id != entity.Id &&
                        template.EmailEventId == entity.EmailEventId &&
                        template.CultureName == entity.CultureName &&
                        template.TenantId == entity.TenantId &&
                        template.OrganizationId == entity.OrganizationId &&
                        template.IsActive,
            throwIfNotExists: false,
            cancellationToken);
        if (current is not null)
            current.IsActive = false;
    }

    // Konu ve icerigin soz dizimini dogrulayip girdi alanlarini veri kabuguna uygular.
    private static void Apply(EmailTemplate entity, EmailTemplateInputModel model)
    {
        EnsureValidSyntax(model.Subject);
        EnsureValidSyntax(model.Body);
        entity.Name = NormalizeRequiredText(model.Name);
        entity.Subject = NormalizeRequiredText(model.Subject);
        entity.Body = model.Body;
    }

    // Hatali sablonun kaydedilmesini engeller; gonderim aninda render hatasi olusmaz.
    private static void EnsureValidSyntax(string content)
    {
        if (Template.Parse(content).HasErrors)
            throw new BusinessException(EmailTemplateExceptionCodes.InvalidTemplateSyntax);
    }

    // Sablonu, modelin uyeleri adlari degismeden ust duzey degisken olacak sekilde doldurur ({{ name }}).
    private static async Task<string> RenderAsync(
        string content,
        object model,
        CancellationToken cancellationToken = default)
    {
        var globals = new ScriptObject();
        globals.Import(model, renamer: member => member.Name);
        var context = new TemplateContext
        {
            MemberRenamer = member => member.Name,
            CancellationToken = cancellationToken
        };
        context.PushGlobal(globals);
        return await Template.Parse(content).RenderAsync(context);
    }
}
