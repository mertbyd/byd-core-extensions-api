using Byd.CoreExtensions.Localization;
using Volo.Abp.Authorization.Permissions;

namespace Byd.CoreExtensions.Permissions;

// islevi: E-posta sablonu yetkilerini ABP yetki agacina ekler.
// sistemdeki gorevi: Ana provider'in Define metodu bu partial metodu cagirir. Sablon satiri istegi yapanin kapsamina
// damgalanir ve her uc yalniz o kapsamin satirlarini gorur; tenant ve organizasyon yoneticileri kendi sablonlarini
// yonetebilsin diye yetki iki tarafta da verilebilir.
public partial class CoreExtensionsPermissionDefinitionProvider
{
    private void AddEmailTemplatesPermissions(PermissionGroupDefinition group)
    {
        var emailTemplates = group.AddPermission(
            CoreExtensionsPermissions.EmailTemplates.Default,
            L(CoreExtensionsLocalizationConsts.Permissions.EmailTemplates));

        emailTemplates.AddChild(
            CoreExtensionsPermissions.EmailTemplates.View,
            L(CoreExtensionsLocalizationConsts.Permissions.EmailTemplatesView));

        emailTemplates.AddChild(
            CoreExtensionsPermissions.EmailTemplates.Manage,
            L(CoreExtensionsLocalizationConsts.Permissions.EmailTemplatesManage));
    }
}
