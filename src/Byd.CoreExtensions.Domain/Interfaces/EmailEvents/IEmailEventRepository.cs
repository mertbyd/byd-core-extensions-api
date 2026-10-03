using Byd.CoreExtensions.Entities.EmailEvents;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// E-posta olayi lookup'i icin extended repository sozlesmesini tanimlar.
/// </summary>
// islevi: EmailEvent okuma ve yazma yeteneklerini Domain katmanina soyutlar.
// sistemdeki gorevi: EmailEventManager ve olayi seed eden moduller ile EF Core repository uygulamasi arasindaki veri
// erisim siniridir.
public interface IEmailEventRepository : IExtendedRepository<EmailEvent>
{
}
