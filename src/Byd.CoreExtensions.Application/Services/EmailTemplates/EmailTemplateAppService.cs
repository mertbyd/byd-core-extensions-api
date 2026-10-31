using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Byd.CoreExtensions.Dtos.EmailTemplates;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Mappers.EmailTemplates;

namespace Byd.CoreExtensions.Services.EmailTemplates;

/// <summary>
/// E-posta sablonlarinin CRUD ve aktiflestirme orkestrasyonunu yurutur.
/// </summary>
// islevi: Girdi dogrulamasini, Manager kararlarini ve repository kaydini duz akisla birlestirir.
// sistemdeki gorevi: Controller ile EmailTemplate Manager/Repository zinciri arasindaki tek public giris noktasidir.
public class EmailTemplateAppService : CoreExtensionsAppService, IEmailTemplateAppService
{
    private static readonly EmailTemplateMapper Mapper = new();

    private EmailTemplateManager Manager =>
        LazyGetRequiredService<EmailTemplateManager>();

    private IEmailTemplateRepository Repository =>
        LazyGetRequiredService<IEmailTemplateRepository>();

    private IValidator<CreateEmailTemplateDto> CreateValidator =>
        LazyGetRequiredService<IValidator<CreateEmailTemplateDto>>();

    private IValidator<UpdateEmailTemplateDto> UpdateValidator =>
        LazyGetRequiredService<IValidator<UpdateEmailTemplateDto>>();
    // Kimligi verilen sablonu Manager'in kapsam guard'i uzerinden getirir.
    public virtual async Task<EmailTemplateDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Manager.EnsureExistsInScopeAsync(id, cancellationToken);
        return Mapper.Map(entity);
    }
    // Istegin kapsamindaki tum sablon surumlerini getirir.
    public virtual async Task<List<EmailTemplateDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Manager.GetListInScopeAsync(cancellationToken);
        return Mapper.Map(entities);
    }
    // Create girdisini dogrular, Manager ile aktif sablonu kurar ve tek UOW icinde kaydeder.
    public virtual async Task<EmailTemplateDto> CreateAsync(CreateEmailTemplateDto input, CancellationToken cancellationToken = default)
    {
        await CreateValidator.ValidateAndThrowAsync(input, cancellationToken);
        var model = Mapper.Map(input);
        var entity = await Manager.CreateAsync(model, cancellationToken);
        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }
    // Update girdisini dogrular, kapsamdaki sablonu Manager ile degistirir ve kaydeder.
    public virtual async Task<EmailTemplateDto> UpdateAsync(
        Guid id,
        UpdateEmailTemplateDto input,
        CancellationToken cancellationToken = default)
    {
        await UpdateValidator.ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.EnsureExistsInScopeAsync(id, cancellationToken);
        var model = Mapper.Map(input);
        Manager.Update(entity, model);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }
    // Kapsamdaki pasif sablonu Manager'in silme kuralindan sonra kalici olarak siler.
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Manager.EnsureExistsInScopeAsync(id, cancellationToken);
        Manager.EnsureCanDelete(entity);
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }
    // Kapsamdaki pasif sablonu aktif yapar; Manager onceki aktif surumu ayni UOW icinde pasife alir.
    public virtual async Task<EmailTemplateDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Manager.EnsureExistsInScopeAsync(id, cancellationToken);
        await Manager.ActivateAsync(entity, cancellationToken);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }
}
