namespace Byd.CoreExtensions;

public static class CoreExtensionsDbProperties
{
    public static string DbTablePrefix { get; set; } = "CoreExtensions";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "CoreExtensions";
}
