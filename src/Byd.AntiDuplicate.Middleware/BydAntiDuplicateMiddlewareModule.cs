using Byd.CoreExtensions;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace Byd.AntiDuplicate.Middleware;

/// <summary>
/// Mukerrer istek korumasinin ABP modul kompozisyonu.
/// </summary>
// islevi: Korumanin bagimli oldugu sabit, ayar ve localization kaynagini ve dagitik cache altyapisini modul zincirine baglar.
// sistemdeki gorevi: Korumayi tuketen host'a acan modul. Middleware konvansiyonel kayitla geldigi icin burada ayrica kaydedilmez; pipeline'a baglama isi tuketen tarafa aittir.
[DependsOn(
    typeof(CoreExtensionsDomainSharedModule),
    typeof(AbpCachingModule)
    )]
public class BydAntiDuplicateMiddlewareModule : AbpModule
{
}
