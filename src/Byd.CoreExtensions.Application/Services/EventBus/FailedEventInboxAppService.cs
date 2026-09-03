using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Byd.CoreExtensions.Dtos.EventBus;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Managers;
using Byd.CoreExtensions.Mappers.EventBus;

namespace Byd.CoreExtensions.Services.EventBus;

/// <summary>
/// Basarisiz olay kutusu okuma orkestrasyonunu yurutur.
/// </summary>
// islevi: Liste girdisini dogrular, Manager varlik kontrolunu ve repository sorgularini Mapperly ciktisiyla birlestirir.
// sistemdeki gorevi: Controller ile FailedEventInbox Manager/Repository zinciri arasindaki tek public giris noktasidir.
public class FailedEventInboxAppService
    : CoreExtensionsAppService, IFailedEventInboxAppService
{
    private static readonly FailedEventInboxMapper Mapper = new();
    private FailedEventInboxManager Manager =>
        LazyGetRequiredService<FailedEventInboxManager>();
    private IFailedEventInboxRepository Repository =>
        LazyGetRequiredService<IFailedEventInboxRepository>();
    private IValidator<FailedEventInboxListInput> ListValidator =>
        LazyGetRequiredService<IValidator<FailedEventInboxListInput>>();
    // Kimligi verilen basarisiz olay kaydini Manager guard'i uzerinden getirir.
    public virtual async Task<FailedEventInboxDto> GetAsync(Guid id)
    {
        var entity = (await Manager.TryGetByIdAsync(
            id,
            throwIfNotExists: true,
            CancellationTokenProvider.Token))!;
        return Mapper.Map(entity);
    }
    // Liste girdisini dogrular, dinamik sorguyu repository'de calistirir ve sonucu esler.
    public virtual async Task<List<FailedEventInboxDto>> GetFilteredListAsync(
        FailedEventInboxListInput input)
    {
        var cancellationToken = CancellationTokenProvider.Token;
        await ListValidator.ValidateAndThrowAsync(input, cancellationToken);
        var entities = await Repository.GetFilteredListAsync(input, cancellationToken);
        return Mapper.Map(entities);
    }
    // Liste girdisini dogrular ve ayni filtreye uyan repository kayit sayisini dondurur.
    public virtual async Task<int> GetFilteredCountAsync(FailedEventInboxListInput input)
    {
        var cancellationToken = CancellationTokenProvider.Token;
        await ListValidator.ValidateAndThrowAsync(input, cancellationToken);
        return await Repository.GetFilteredCountAsync(input, cancellationToken);
    }
}
