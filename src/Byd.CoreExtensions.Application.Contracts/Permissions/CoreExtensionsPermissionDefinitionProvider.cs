using Byd.CoreExtensions.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Byd.CoreExtensions.Permissions;

/// <summary>
/// Modulun yetki agacini ABP'ye tanimlar.
/// </summary>
// islevi: Yetki grubunu acar ve her ozellik ailesinin kendi partial metodunu cagirir.
// sistemdeki gorevi: Yeni bir ozellik ailesi eklemek bu dosyaya tek satir eklemekten ibarettir;
// mevcut satirlar degismez.
public partial class CoreExtensionsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            CoreExtensionsPermissions.GroupName,
            L(CoreExtensionsLocalizationConsts.Permissions.Group));

        AddModuleSettingsPermissions(group);
        AddApiKeysPermissions(group);
        AddFailedEventInboxesPermissions(group);
    }

    // Localization anahtarini modul kaynagina baglar.
    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CoreExtensionsResource>(name);
    }
}
