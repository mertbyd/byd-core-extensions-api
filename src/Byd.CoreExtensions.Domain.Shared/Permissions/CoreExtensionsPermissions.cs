using Volo.Abp.Reflection;

namespace Byd.CoreExtensions.Permissions;

/// <summary>
/// Modulun yetki sabitlerinin kokudur.
/// </summary>
// islevi: Yetki grubunun adini tanimlar ve tum public yetki sabitlerini tek listede toplar.
// sistemdeki gorevi: Yetki string'lerinin tek kaynagi; ozellik aileleri partial dosyalarda eklenir,
// bu dosya degismeden buyur.
public partial class CoreExtensionsPermissions
{
    public const string GroupName = "CoreExtensions";

    // Tum public yetki sabitlerini ABP kayit ve test altyapisina acar.
    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(CoreExtensionsPermissions));
    }
}
