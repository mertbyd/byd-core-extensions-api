using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.ApiKeys;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Gelen API anahtarini bellekteki guncel goruntuye karsi dogrulayan depo sozlesmesi.
/// </summary>
// islevi: Goruntunun tazeligini denetler, gerekiyorsa tabloyu bastan okutur, anahtari arar ve son kullanma kontrolunu yapar.
// sistemdeki gorevi: Anahtar dogrulamasinin tek giris noktasidir.
public interface IApiKeyStore
{
    // Gelen anahtari guncel goruntuye karsi dogrular; goruntu bayatsa once tazelenir.
    Task<ApiKeyValidationResult> ValidateAsync(string? key, CancellationToken cancellationToken = default);
}
