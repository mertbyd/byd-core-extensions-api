using Byd.CoreExtensions.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.Permissions;

// islevi: Basarisiz olay kaydi yetkilerini ABP yetki agacina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir. Olay kutusu kayitlari altyapiya
// aittir ve tenant'a bagli degildir; yetki yalniz host tarafinda verilebilir.
public partial class CoreExtensionsPermissionDefinitionProvider
{
    private void AddFailedEventInboxesPermissions(PermissionGroupDefinition group)
    {
        var failedEventInboxes = group.AddPermission(
            CoreExtensionsPermissions.FailedEventInboxes.Default,
            L(CoreExtensionsLocalizationConsts.Permissions.FailedEventInboxes),
            MultiTenancySides.Host);

        failedEventInboxes.AddChild(
            CoreExtensionsPermissions.FailedEventInboxes.View,
            L(CoreExtensionsLocalizationConsts.Permissions.FailedEventInboxesView),
            MultiTenancySides.Host);
    }
}
