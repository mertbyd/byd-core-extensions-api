using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Byd.CoreExtensions.Dtos.ApiKeys;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Mappers.ApiKeys;

namespace Byd.CoreExtensions.Services.ApiKeys;

/// <summary>
/// API anahtari okuma ve uretme orkestrasyonunu yurutur.
/// </summary>
// islevi: Girdi dogrulamasini, Manager mutasyonunu ve repository kaydini duz akisla birlestirir.
// sistemdeki gorevi: Controller ile ApiKey Manager/Repository zinciri arasindaki tek public giris noktasidir.
public class ApiKeyAppService : CoreExtensionsAppService, IApiKeyAppService
{
    private static readonly ApiKeyMapper Mapper = new();

    private ApiKeyManager Manager =>
        LazyGetRequiredService<ApiKeyManager>();

    private IApiKeyRepository Repository =>
        LazyGetRequiredService<IApiKeyRepository>();

    private IValidator<CreateApiKeyDto> CreateValidator =>
        LazyGetRequiredService<IValidator<CreateApiKeyDto>>();
    // Kimligi verilen API anahtarini Manager guard'i uzerinden getirir.
    public virtual async Task<ApiKeyDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Manager.EnsureExistsAsync(id, cancellationToken);
        return Mapper.Map(entity);
    }
    // Create girdisini dogrular, Manager ile anahtar uretir ve tek UOW icinde kaydeder.
    // Uretilen anahtar degeri yalnizca bu cevapta doner; sonraki okumalar anahtari tasimaz.
    public virtual async Task<CreatedApiKeyDto> CreateAsync(
        CreateApiKeyDto input,
        CancellationToken cancellationToken = default)
    {
        await CreateValidator.ValidateAndThrowAsync(input, cancellationToken);
        var model = Mapper.Map(input);
        var entity = Manager.Create(model);
        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return Mapper.MapCreated(entity);
    }
}
