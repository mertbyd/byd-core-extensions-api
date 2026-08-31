using System.Collections.Generic;
namespace Byd.CoreExtensions.Options;
/// <summary>
/// Modulu tuketen host'un beyan ettigi calisma zamani secenekleri.
/// </summary>
// islevi: Modulun ayar satiri adini ve anti-duplicate korumasi disinda tutulacak yol on eklerini host beyanindan tasir.
// sistemdeki gorevi: NuGet olarak tuketen her host'un kendi ayar satirini ve muafiyetlerini acikca bildirdigi tek sozlesme; modul adi ne assembly'den tahmin edilir ne de sabit varsayilana duser.
public class CoreExtensionsModuleOptions
{
    // Bu host'un ModuleSettings tablosundaki satir adi; beyan edilmezse cozumleme acilista hata verir.
    public string? ModuleName { get; set; }
    // Anti-duplicate korumasi disinda tutulacak yol on ekleri; bos birakilirsa hicbir yol muaf degildir.
    public List<string> ExcludePaths { get; } = new();
}
