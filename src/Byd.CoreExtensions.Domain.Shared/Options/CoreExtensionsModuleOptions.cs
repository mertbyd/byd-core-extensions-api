using System.Collections.Generic;
namespace Byd.CoreExtensions.Options;
/// <summary>
/// Modulu tuketen host'un beyan ettigi calisma zamani secenekleri.
/// </summary>
// islevi: Modulun ayar satiri adini ve anti-duplicate korumasi disinda tutulacak beyanlari host beyanindan tasir.
// sistemdeki gorevi: NuGet olarak tuketen her host'un kendi ayar satirini ve muafiyetlerini acikca bildirdigi tek sozlesme; modul adi ne assembly'den tahmin edilir ne de sabit varsayilana duser.
public class CoreExtensionsModuleOptions
{
    // Bu host'un ModuleSettings tablosundaki satir adi; beyan edilmezse cozumleme acilista hata verir.
    public string? ModuleName { get; set; }
    // Bu host'un calistigi ortam adi; olay kutusu kayitlarinin sahiplik kapsamini ayirir.
    // Ayni sema uzerinde birden fazla ortam calisirken kayitlarin karismamasini saglar.
    public string? EnvironmentName { get; set; }
    // Anti-duplicate korumasi disinda tutulacak beyanlar; bos birakilirsa hicbir istek muaf degildir.
    // Bir beyan iki seyden biri olabilir:
    //   HTTP metodu  -> "/get" veya "get" o metotla gelen tum istekleri disarida birakir;
    //   yol on eki   -> "/api/orders" o yolun altindaki tum uclari disarida birakir.
    // Karsilastirma buyuk/kucuk harf duyarsizdir. Bir beyan hem metot adina hem yola benziyorsa
    // once metot olarak degerlendirilir; bu yuzden gercekten "/get" adli bir uc varsa yol yerine
    // metot kurali olarak islenir.
    public List<string> ExcludePaths { get; } = new();
}
