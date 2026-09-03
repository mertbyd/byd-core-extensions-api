using System;
using Byd.CoreExtensions.Consts;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectExtending;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Gelen olay kaydinin modul tarafindan eklenen ozelliklerini tanimlar ve kolonlarina baglar.
/// </summary>
// islevi: Kayit sahibi kapsam ozelliklerini once nesne genisletme sozlesmesine yazar, sonra ayni ozellikleri EF Core kolonlarina map eder.
// sistemdeki gorevi: Olay kutusu ek alanlarinin tek tanim sahibi; kalici kayit ile tasima nesnesinin ayni ozellik kumesini gormesini saglar.
//
// NEDEN BURASI. Gelen olay kaydi cerceveden gelir ve kapali bir tiptir; yeni bir property
// eklemek icin kaynagini degistiremeyiz. Cerceve bu durum icin nesne genisletme altyapisi
// sunar: tip IHasExtraProperties tasidigi icin ona calisma aninda ozellik eklenebilir, ve bu
// ozellik istenirse gercek bir kolona baglanabilir. Bu dosya o iki adimin sahibidir.
//
// EKLEDIGIMIZ IKI ALAN:
//   owner_module_name        Ayni veritabanini paylasan farkli kurulumlar birbirinin olayini
//                            islemesin diye. Bekleyen kayit sorgusu bu alana gore filtreler.
//   owner_environment_name   Ayni ayrimin ortam boyutu; test ortami canlinin olayini islemez.
// Uzunluk sinirlari ve kararli adlar EventBoxConsts'tadir; burada string veya sayi literali yoktur.
//
// IKI ADIM VE SIRASI:
//   1. DefineProperties  ozelligin var oldugunu bildirir. Bu olmadan deger yazma ve okuma
//                        cagrilari tanimsiz ozelliğe gider.
//   2. MapColumns        ayni ozelligi gercek bir tabloya kolonu yapar. Bu olmadan deger yalnizca
//                        extra_properties sozluk kolonunda yasar ve uzerinde indeksli sorgu
//                        yazilamaz; bekleyen kayit filtresi tam da bunu gerektirdigi icin sart.
//
// NEDEN IKI TIPE BIRDEN TANIMLANIYOR. Kalici kayit ile cercevenin tasima nesnesi ayri tiplerdir
// ve kutuya yazma ile kutudan okuma sirasinda birbirine cevrilir. Cevrim ek ozellikleri ad ad
// kopyalar; ozellik yalnizca bir tarafta tanimliysa diger tarafa gecerken sessizce dusen deger
// olur. Ikisinde de tanimli olmasi bu kaybi engeller.
//
// KAPSAM DEGERLERI NEDEN VARSAYILAN DEGIL. Varsayilan deger acilista hesaplanmak zorundadir;
// kayit sahibi modul adinin kaynagi ise ancak istek aninda cozulur. Bu yuzden degerler burada
// tutulmaz. Yazma ve okuma taraflari store icinde ayni kaynaklardan deger alir.
public static class EventBoxObjectExtensionConfigurator
{
    // Once ozellik tanimlarini, sonra kolon eslemelerini uygular; sira zorunludur, esleme tanimli ozelligi bekler.
    public static void Configure()
    {
        DefineProperties();
        MapColumns();
    }

    // Ek ozellikleri hem kalici kayit hem de tasima nesnesi uzerinde tanimlar.
    private static void DefineProperties()
    {
        DefineOn<IncomingEventRecord>();
        DefineOn<IncomingEventInfo>();
    }

    // Verilen tipe olay kutusunun dort ek ozelligini kararli adlariyla ekler.
    // Tip parametresi sayesinde kalici kayit ile tasima nesnesi ayni listeden beslenir ve
    // ileride bir alan eklendiginde iki tarafin ayrisma ihtimali kalmaz.
    private static void DefineOn<TObject>()
        where TObject : IHasExtraProperties
    {
        ObjectExtensionManager.Instance
            .AddOrUpdateProperty<TObject, string>(EventBoxConsts.OwnerModuleNamePropertyName)
            .AddOrUpdateProperty<TObject, string>(EventBoxConsts.OwnerEnvironmentNamePropertyName);
    }

    // Kalici kayittaki ek ozellikleri gercek EF Core kolonlarina baglar ve uzunluk sinirlarini verir.
    // Yalnizca kalici kayit map edilir; tasima nesnesinin tablosu yoktur.
    // not_before uzunluk sinirisiz birakilir, tarih kolonudur.
    private static void MapColumns()
    {
        ObjectExtensionManager.Instance
            .MapEfCoreProperty<IncomingEventRecord, string>(
                EventBoxConsts.OwnerModuleNamePropertyName,
                (_, propertyBuilder) =>
                    propertyBuilder.HasMaxLength(EventBoxConsts.MaxOwnerModuleNameLength))
            .MapEfCoreProperty<IncomingEventRecord, string>(
                EventBoxConsts.OwnerEnvironmentNamePropertyName,
                (_, propertyBuilder) =>
                    propertyBuilder.HasMaxLength(EventBoxConsts.MaxOwnerEnvironmentNameLength));
    }
}
