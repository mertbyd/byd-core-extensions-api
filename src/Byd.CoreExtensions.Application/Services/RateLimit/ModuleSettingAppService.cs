using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Byd.CoreExtensions.Dtos.RateLimit;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Mappers.RateLimit;

namespace Byd.CoreExtensions.Services.RateLimit;

/// <summary>
/// Modul ayarlarinin CRUD orkestrasyonunu yurutur.
/// </summary>
// islevi: Girdi dogrulamasini, Manager mutasyonunu ve repository kaydini duz akisla birlestirir.
// sistemdeki gorevi: Controller ile ModuleSetting Manager/Repository zinciri arasindaki tek public giris noktasidir.
public class ModuleSettingAppService : CoreExtensionsAppService, IModuleSettingAppService
{
    private static readonly ModuleSettingMapper Mapper = new();

    private ModuleSettingManager Manager =>
        LazyGetRequiredService<ModuleSettingManager>();

    private IModuleSettingRepository Repository =>
        LazyGetRequiredService<IModuleSettingRepository>();

    private IValidator<CreateModuleSettingDto> CreateValidator =>
        LazyGetRequiredService<IValidator<CreateModuleSettingDto>>();

    private IValidator<UpdateModuleSettingDto> UpdateValidator =>
        LazyGetRequiredService<IValidator<UpdateModuleSettingDto>>();
    // Kimligi verilen modul ayarini Manager guard'i uzerinden getirir.
    public virtual async Task<ModuleSettingDto> GetAsync(Guid id)
    {
        var entity = await Manager.EnsureExistsAsync(id, CancellationTokenProvider.Token);
        return Mapper.Map(entity);
    }
    // Tum modul ayarlarini detay iliskisi yuklemeden repository'den getirir.
    public virtual async Task<List<ModuleSettingDto>> GetListAsync()
    {
        var entities = await Repository.GetListAsync(
            includeDetails: false,
            cancellationToken: CancellationTokenProvider.Token);
        return Mapper.Map(entities);
    }
    // Create girdisini dogrular, Manager ile entity kurar ve tek UOW icinde kaydeder.
    public virtual async Task<ModuleSettingDto> CreateAsync(CreateModuleSettingDto input)
    {
        var cancellationToken = CancellationTokenProvider.Token;
        await CreateValidator.ValidateAndThrowAsync(input, cancellationToken);
        var model = Mapper.Map(input);
        var entity = await Manager.CreateAsync(model, cancellationToken);
        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }
    // Update girdisini dogrular, mevcut entity'yi Manager ile degistirir ve kaydeder.
    public virtual async Task<ModuleSettingDto> UpdateAsync(
        Guid id,
        UpdateModuleSettingDto input)
    {
        var cancellationToken = CancellationTokenProvider.Token;
        await UpdateValidator.ValidateAndThrowAsync(input, cancellationToken);
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken);
        var model = Mapper.Map(input);
        await Manager.UpdateAsync(entity, model, cancellationToken);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.Map(entity);
    }
    // Kimligi verilen modul ayarini varlik guard'i sonrasinda kalici olarak siler.
    public virtual async Task DeleteAsync(Guid id)
    {
        var cancellationToken = CancellationTokenProvider.Token;
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken);
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }
}
