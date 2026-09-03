using Volo.Abp.Threading;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Modulun EF Core nesne genisletme kayitlarini tek noktadan ve bir kez calistirir.
/// </summary>
// islevi: Konu bazli genisletme yapilandiricilarini sirayla cagirir ve tekrarli calismayi engeller.
// sistemdeki gorevi: Modul acilisindaki tek genisletme giris noktasidir; yeni bir konu eklendiginde yalnizca buraya bir cagri girer.
//
// NEDEN AYRI BIR GIRIS NOKTASI. Genisletme kayitlari konu konu buyur; olay kutusundan sonra
// baska bir cerceve tipine de alan eklenebilir. Tanimlar konu dosyalarinda kalir, burasi yalnizca
// hangi konularin kayit edilecegini soyler. Boylece modul sinifi tek satirla bu metodu cagirir ve
// tanimlarin kalabaligini gormez.
//
// NEDEN BIR KEZ. Kayitlar surece ait global bir sozluge yazilir, ornek basina degil. Ayni kayit
// ikinci kez isletilirse ayni ozellik tekrar tanimlanmis olur; testler gibi modulun birden fazla
// kez kuruldugu ortamlarda bu gercek bir senaryodur. OneTimeRunner cagriyi surec omrunde tek
// sefere indirger.
//
// NEDEN ACILISIN EN BASINDA. Bu kayitlar EF Core modeli kurulmadan once tamamlanmis olmali;
// model kurulurken okunurlar. Cagri bu yuzden modulun PreConfigureServices asamasindadir,
// ConfigureServices'te degil.
public static class CoreExtensionsEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new();

    // Konu yapilandiricilarini surec omrunde yalnizca bir kez uygular.
    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            EventBoxObjectExtensionConfigurator.Configure();
        });
    }
}
