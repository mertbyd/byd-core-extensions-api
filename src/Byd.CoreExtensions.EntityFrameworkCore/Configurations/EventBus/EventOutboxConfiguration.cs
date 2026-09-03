using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Consts;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Giden olay kutusu tablosunun modul semasindaki adini tanimlar.
/// </summary>
// islevi: Cerceveden hazir gelen giden-olay kaydini kararli tablo adina ve modul semasina map eder.
// sistemdeki gorevi: Kutu tablosunun modul semasinda dogmasini saglar; kolon ve indeks sozlesmesine karismaz.
//
// ENTITY BIZIM DEGIL. Tabloyu tasiyan OutgoingEventRecord cerceveden gelir, bu depoda tanimli
// degildir. Gelen kutusuyla ayni gerekce: kutuyu bosaltan gonderici de cercevenin tipiyle calisir.
//
// TABLO SU KOLONLARLA HAZIR GELIYOR (hicbiri burada tekrar tanimlanmaz):
//   id                 Guid, birincil anahtar
//   event_name         Olayin adi, en fazla OutgoingEventRecord.MaxEventNameLength (256) karakter, zorunlu
//   event_data         Olayin serilestirilmis govdesi, byte[], zorunlu
//   creation_time      Kaydin olusma zamani; gonderim sirasi bu alana gore kurulur
//   extra_properties   Genisletme alanlarinin sozluk kolonu
//   concurrency_stamp  Es zamanli guncelleme damgasi
// Indeks de hazir gelir: creation_time.
//
// GIDEN KUTUYA EK KOLON EKLEMEDIK. Gelen kutudaki dort ek alan yalnizca gelen tarafta anlamli:
//   solution_name / environment_name  hangi kurulumun isleyecegini secmek icindir, giden olayi
//                                     zaten biz uretiriz, kapsam ayirmaya gerek yoktur
//   not_before                        gecikmeli islemedir, gonderimde karsiligi yoktur
//   external_reference_code           gelen olayin dis sistem korelasyonudur
// Giden tarafa ihtiyac dogarsa ekleme yeri EventBoxObjectExtensionConfigurator olur, burasi degil.
//
// BURADA YALNIZCA TABLO ADI VE SEMA EZILIR; gerekce ve sira icin gelen kutu konfigurasyonuna bak.
public sealed class EventOutboxConfiguration : IEntityTypeConfiguration<OutgoingEventRecord>
{
    // Giden olay kutusunun tablo adini ve semasini uygular; kolon ve indeks tanimina girmez.
    public void Configure(EntityTypeBuilder<OutgoingEventRecord> builder)
    {
        builder.ToTable(
            EventBoxConsts.OutboxTableName,
            CoreExtensionsDbProperties.DbSchema);
    }
}
