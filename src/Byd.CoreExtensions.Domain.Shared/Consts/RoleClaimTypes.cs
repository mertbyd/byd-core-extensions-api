namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Kapsamdaki rollerin kimliklerinin token uzerinde tasindigi claim adi.
/// </summary>
// islevi: Rol kimligini token'a yazan taraf ile istekte okuyan tarafin ayni claim adini kullanmasini saglar.
// sistemdeki gorevi: Cercevenin rol claim'i adi tasir; degismeyen kimlik her rol icin ayri bir degerde tasinir.
public static class RoleClaimTypes
{
    // Kapsamdaki bir rolun kimligi.
    public const string RoleId = "role_id";
}
