using System;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Providers;
using Volo.Abp.Domain;
using Volo.Abp.Emailing;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using Volo.Abp.Timing;

namespace Byd.CoreExtensions;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(CoreExtensionsDomainSharedModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpEmailingModule)
)]
// islevi: Domain katmaninin ABP modul kompozisyonunu, zaman kaynagi ayarini ve organizasyon kapsaminin
// saklama noktasini kurar. E-posta sablonlari ABP'nin ayarlardan beslenen SMTP gondericisiyle gonderilir.
// sistemdeki gorevi: Entity denetim alanlarinin hangi DateTimeKind ile uretilecegini belirleyen tek yerdir;
// kapsam saklama noktasinin tekil kaydi da buradan gelir.
public class CoreExtensionsDomainModule : AbpModule
{
    // ABP saatini UTC'ye sabitler ve organizasyon kapsaminin saklama noktasini kaydeder.
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // PostgreSQL "timestamp with time zone" kolonlari yalnizca UTC kabul eder; ABP varsayilani
        // Kind=Unspecified/Local uretir ve CreationTime/LastModificationTime yazimi ArgumentException
        // ile duser. Kind burada sabitlenince hem Clock.Now hem denetim alanlari UTC olarak normalize edilir.
        Configure<AbpClockOptions>(options => options.Kind = DateTimeKind.Utc);

        // Kapsam saklama noktasi kendi tekil ornegini tasir; kayit bu yuzden konvansiyonel degil,
        // o ornek uzerinden aciktan yapilir. Tenant tarafindaki karsiligi da ayni sekilde kaydedilir.
        context.Services.AddSingleton<ICurrentOrganizationAccessor>(AsyncLocalCurrentOrganizationAccessor.Instance);
    }
}
