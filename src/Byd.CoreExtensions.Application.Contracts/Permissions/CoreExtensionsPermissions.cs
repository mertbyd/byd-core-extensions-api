using Volo.Abp.Reflection;

namespace Byd.CoreExtensions.Permissions;

public class CoreExtensionsPermissions
{
    public const string GroupName = "CoreExtensions";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(CoreExtensionsPermissions));
    }
}
