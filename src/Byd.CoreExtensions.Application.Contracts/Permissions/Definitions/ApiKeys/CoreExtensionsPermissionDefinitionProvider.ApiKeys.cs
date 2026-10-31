using Byd.CoreExtensions.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.Permissions;

// islevi: API anahtari yetkilerini ABP yetki agacina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir. API anahtarlari tenant filtresine
// tabi degildir ve anahtarin tenant ile organizasyonu istekte secilir; tenant yoneticisi baska kapsama anahtar
// uretemesin diye yetki yalniz host tarafinda verilebilir.
public partial class CoreExtensionsPermissionDefinitionProvider
{
    private void AddApiKeysPermissions(PermissionGroupDefinition group)
    {
        var apiKeys = group.AddPermission(
            CoreExtensionsPermissions.ApiKeys.Default,
            L(CoreExtensionsLocalizationConsts.Permissions.ApiKeys),
            MultiTenancySides.Host);

        apiKeys.AddChild(
            CoreExtensionsPermissions.ApiKeys.View,
            L(CoreExtensionsLocalizationConsts.Permissions.ApiKeysView),
            MultiTenancySides.Host);

        apiKeys.AddChild(
            CoreExtensionsPermissions.ApiKeys.Manage,
            L(CoreExtensionsLocalizationConsts.Permissions.ApiKeysManage),
            MultiTenancySides.Host);
    }
}
