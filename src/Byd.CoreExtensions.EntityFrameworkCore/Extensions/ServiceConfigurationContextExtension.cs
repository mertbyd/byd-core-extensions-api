using Byd.CoreExtensions.Consts;
using Volo.Abp.Data;
using Volo.Abp.SettingManagement;

namespace Byd.CoreExtensions.Extensions;

/// <summary>
/// ABP framework tablolarinin sema ve tablo on ekini modulun sozlesmesine gore ezer.
/// </summary>
// islevi: ABP'nin statik DbProperties degerlerini modelin kurulmasindan once abp semasina yonlendirir.
// sistemdeki gorevi: Modul bir ana uygulamaya baglandiginda ABP tablolari public semasinda kalmasin diye
// host, EF modelini kurmadan once bu cozumu cagirir.
public static class ServiceConfigurationContextExtension
{
    // ABP ortak ve ayar yonetimi tablolarini abp semasina alir; tablo on ekini kaldirir.
    public static void ResolveSchemaAndPrefix()
    {
        AbpCommonDbProperties.DbSchema = DatabaseConsts.AbpSchema;
        AbpCommonDbProperties.DbTablePrefix = string.Empty;

        AbpSettingManagementDbProperties.DbSchema = DatabaseConsts.AbpSchema;
        AbpSettingManagementDbProperties.DbTablePrefix = string.Empty;
    }
}
