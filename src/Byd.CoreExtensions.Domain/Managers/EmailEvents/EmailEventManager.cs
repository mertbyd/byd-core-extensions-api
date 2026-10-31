using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities.EmailEvents;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.EmailEvents;

namespace Byd.CoreExtensions.Managers;

/// <summary>
/// E-posta olayi normalizasyon, varlik ve benzersizlik kurallarini yonetir.
/// </summary>
// islevi: Olay adini kanoniklestirir, ad benzersizligini dogrular ve entity kurar.
// sistemdeki gorevi: Olayi seed eden modulleri ve sablon kurallarini is kararindan, data-shell entity'yi davranistan
// uzak tutan domain sahibidir.
public class EmailEventManager : BaseManager<EmailEvent, Guid>
{
    // EmailEvent extended repository'sini ortak guard tabanina devreder.
    public EmailEventManager(IEmailEventRepository repository)
        : base(repository)
    {
    }

    // Kimligi verilen olayi getirir; yoksa BaseManager lokalize NotFound firlatir.
    public virtual async Task<EmailEvent> EnsureExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return (await TryGetByIdAsync(id, throwIfNotExists: true, cancellationToken))!;
    }

    // Olayi gonderen modulun sabitindeki adla bulur; olay seed edilmediyse null doner.
    public virtual Task<EmailEvent?> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return TryGetByAsync(emailEvent => emailEvent.Name == name, throwIfNotExists: false, cancellationToken);
    }

    // Create modelini normalize edip benzersiz adli yeni EmailEvent entity'sine donusturur.
    // Ad, gonderimde olayin bulundugu anahtardir; ayni ad ikinci kez acilirsa gonderim yanlis olaya duserdi.
    public virtual async Task<EmailEvent> CreateAsync(
        EmailEventCreateModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        var name = NormalizeRequiredText(model.Name);
        await CheckAlreadyExistsAsync(emailEvent => emailEvent.Name == name, cancellationToken);
        return new EmailEvent(GuidGenerator.Create())
        {
            Name = name,
            Code = model.Code
        };
    }
}
