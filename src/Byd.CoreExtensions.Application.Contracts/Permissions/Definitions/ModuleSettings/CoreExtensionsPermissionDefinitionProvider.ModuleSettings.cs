using Byd.CoreExtensions.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.Permissions;

// islevi: Modul ayari yetkilerini ABP yetki agacina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir. Modul ayari satirlari tenant
// filtresine tabi degildir ve hedef tenant istekte secilir; tenant yoneticisi diger tenant'larin satirlarina
// erismesin diye yetki yalniz host tarafinda verilebilir.
public partial class CoreExtensionsPermissionDefinitionProvider
{
    private void AddModuleSettingsPermissions(PermissionGroupDefinition group)
    {
        var moduleSettings = group.AddPermission(
            CoreExtensionsPermissions.ModuleSettings.Default,
            L(CoreExtensionsLocalizationConsts.Permissions.ModuleSettings),
            MultiTenancySides.Host);

        moduleSettings.AddChild(
            CoreExtensionsPermissions.ModuleSettings.View,
            L(CoreExtensionsLocalizationConsts.Permissions.ModuleSettingsView),
            MultiTenancySides.Host);

        moduleSettings.AddChild(
            CoreExtensionsPermissions.ModuleSettings.Manage,
            L(CoreExtensionsLocalizationConsts.Permissions.ModuleSettingsManage),
            MultiTenancySides.Host);
    }
}
