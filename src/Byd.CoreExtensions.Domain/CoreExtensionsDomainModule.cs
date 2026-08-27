using System;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using Volo.Abp.Timing;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(CoreExtensionsDomainSharedModule),
    typeof(AbpSettingManagementDomainModule)
)]
// islevi: Domain katmaninin ABP modul kompozisyonunu ve zaman kaynagi ayarini kurar.
// sistemdeki gorevi: Entity denetim alanlarinin hangi DateTimeKind ile uretilecegini belirleyen tek yerdir.
public class CoreExtensionsDomainModule : AbpModule
{
    // ABP saatini UTC'ye sabitler.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // PostgreSQL "timestamp with time zone" kolonlari yalnizca UTC kabul eder; ABP varsayilani
        // Kind=Unspecified/Local uretir ve CreationTime/LastModificationTime yazimi ArgumentException
        // ile duser. Kind burada sabitlenince hem Clock.Now hem denetim alanlari UTC olarak normalize edilir.
        Configure<AbpClockOptions>(options => options.Kind = DateTimeKind.Utc);
    }
}
