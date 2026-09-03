using Microsoft.Extensions.Options;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
namespace Byd.CoreExtensions.Providers;
/// <summary>
/// Host'un beyan ettigi ortam adini cozer.
/// </summary>
// islevi: CoreExtensionsModuleOptions icindeki EnvironmentName beyanini okur; beyan bos ise lokalize BusinessException atar.
// sistemdeki gorevi: Olay kutusu yazma ve okuma yollarinin ayni ortam adini tek kaynaktan okumasini saglar. Adi barindirma ortamindan tahmin etmez; beyan etmemis bir host sessizce baska bir ortamin kayitlarini islemek yerine ilk okumada hata verir.
public class CurrentEnvironmentNameProvider : ISingletonDependency
{
    private readonly IOptions<CoreExtensionsModuleOptions> _options;
    public CurrentEnvironmentNameProvider(IOptions<CoreExtensionsModuleOptions> options)
    {
        _options = options;
    }
    // Beyan edilen ortam adi; host bildirmediyse okuma noktasinda hata verir.
    public string Value => ResolveEnvironmentName();
    // Beyani okur; bos birakilmasini kullanici hatasi degil, dagitim hatasi olarak raporlar.
    private string ResolveEnvironmentName()
    {
        var environmentName = _options.Value.EnvironmentName;
        if (string.IsNullOrWhiteSpace(environmentName))
            throw new BusinessException(ModuleSettingExceptionCodes.EnvironmentNameNotConfigured);
        return environmentName;
    }
}
