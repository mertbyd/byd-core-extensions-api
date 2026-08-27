using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// API anahtari aggregate'i icin extended repository sozlesmesini tanimlar.
/// </summary>
// islevi: ApiKey CRUD ve sorgu yeteneklerine ek olarak anahtar degerine gore tekil okumayi soyutlar.
// sistemdeki gorevi: Anahtar dogrulayan taraf ile EF Core uygulamasi arasindaki veri erisim siniridir.
public interface IApiKeyRepository : IExtendedRepository<ApiKey>
{
    // Anahtar degerine gore tek kaydi dondurur; kayit yoksa null doner.
    Task<ApiKey?> FindByKeyAsync(string key, CancellationToken cancellationToken = default);
}
