using Byd.CoreExtensions.Consts;

namespace Byd.CoreExtensions;

public static class CoreExtensionsDbProperties
{
    public static string DbTablePrefix { get; set; } = "CoreExtensions";

    // Modulun tum tablolari bu semaya yazilir; deger sema adlarinin tek sahibi olan DatabaseConsts'tan gelir.
    public static string? DbSchema { get; set; } = DatabaseConsts.ModuleSchema;

    public const string ConnectionStringName = "CoreExtensions";
}
