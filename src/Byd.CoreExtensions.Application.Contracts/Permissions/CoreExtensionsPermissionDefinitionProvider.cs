using Byd.CoreExtensions.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Byd.CoreExtensions.Permissions;

public class CoreExtensionsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(CoreExtensionsPermissions.GroupName, L("Permission:CoreExtensions"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CoreExtensionsResource>(name);
    }
}
