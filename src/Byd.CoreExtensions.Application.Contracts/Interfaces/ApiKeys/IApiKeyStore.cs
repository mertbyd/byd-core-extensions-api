using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ApiKeys;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Gelen API anahtarini dogrulayan ve cozulmus anahtarlari surec omru boyunca bellekte tutan depo sozlesmesi.
/// </summary>
// islevi: Anahtari once bellekte arar, bulamazsa repository'den okuyup bellege alir ve son kullanma kontrolunu yapar.
// sistemdeki gorevi: Anahtar dogrulamasinin tek giris noktasidir.
public interface IApiKeyStore
{
    // Gelen anahtari dogrular; bellekte yoksa repository'den okuyup bellege alir.
    Task<ApiKeyValidationResult> ValidateAsync(string? key, CancellationToken cancellationToken = default);
}
