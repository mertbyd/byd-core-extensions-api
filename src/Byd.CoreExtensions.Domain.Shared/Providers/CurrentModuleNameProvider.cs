using Microsoft.Extensions.Options;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
namespace Byd.CoreExtensions.Providers;
/// <summary>
/// Host'un beyan ettigi ModuleSettings satir adini cozer.
/// </summary>
// islevi: CoreExtensionsModuleOptions icindeki ModuleName beyanini okur; beyan bos ise lokalize BusinessException atar.
// sistemdeki gorevi: Filtre, rate limit tazeleyici ve seeder'in ayni satir adini tek kaynaktan okumasini saglar. Adi tahmin etmez ve sabit varsayilana dusmez; beyan etmemis bir host sessizce yanlis satiri okumak yerine ilk okumada hata verir.
public class CurrentModuleNameProvider : ISingletonDependency
{
    private readonly IOptions<CoreExtensionsModuleOptions> _options;
    public CurrentModuleNameProvider(IOptions<CoreExtensionsModuleOptions> options)
    {
        _options = options;
    }
    // Beyan edilen satir adi; host bildirmediyse okuma noktasinda hata verir.
    public string Value => ResolveModuleName();
    // Beyani okur; bos birakilmasini kullanici hatasi degil, dagitim hatasi olarak raporlar.
    private string ResolveModuleName()
    {
        var moduleName = _options.Value.ModuleName;
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new BusinessException(ModuleSettingExceptionCodes.ModuleNameNotConfigured);
        return moduleName;
    }
}
