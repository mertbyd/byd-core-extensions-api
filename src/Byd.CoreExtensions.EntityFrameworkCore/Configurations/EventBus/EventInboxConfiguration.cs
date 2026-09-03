using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Byd.CoreExtensions.Consts;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// Gelen olay kutusu tablosunun modul semasindaki adini tanimlar.
/// </summary>
// islevi: Cerceveden hazir gelen gelen-olay kaydini kararli tablo adina ve modul semasina map eder.
// sistemdeki gorevi: Kutu tablosunun modul semasinda dogmasini saglar; kolon ve indeks sozlesmesine karismaz.
//
// ENTITY BIZIM DEGIL. Tabloyu tasiyan IncomingEventRecord cerceveden gelir, bu depoda tanimli
// degildir. Kendi entity'mizi yazmadik cunku kutuyu okuyup yazan isleyiciler de cercevenin
// kendi tipleriyle calisir; paralel bir entity acmak o isleyicileri bastan yazmayi gerektirirdi.
//
// TABLO SU KOLONLARLA HAZIR GELIYOR (hicbiri burada tekrar tanimlanmaz):
//   id                 Guid, birincil anahtar
//   message_id         Kuyruk mesajinin kimligi; ayni mesajin tekrar islenmesini engelleyen alan
//   event_name         Olayin adi, en fazla IncomingEventRecord.MaxEventNameLength (256) karakter, zorunlu
//   event_data         Olayin serilestirilmis govdesi, byte[], zorunlu
//   creation_time      Kaydin olusma zamani
//   status             Isleme durumu; bekleyen kayitlar bu alana gore secilir
//   handled_time       Kaydin ele alindigi zaman
//   retry_count        Cercevenin kendi yeniden deneme sayaci
//   next_retry_time    Cercevenin bir sonraki deneme zamani
//   extra_properties   Genisletme alanlarinin sozluk kolonu
//   concurrency_stamp  Es zamanli guncelleme damgasi
// Indeksler de hazir gelir: (status, creation_time) ve message_id.
//
// BIZIM EKLEDIGIMIZ DORT KOLON burada degil, EventBoxObjectExtensionConfigurator icinde
// tanimlanir: owner_module_name, owner_environment_name, not_before, external_reference_code.
// Nedeni: bunlar cerceve tipinin ozellikleri degil, nesne genisletme altyapisi uzerinden
// eklenen alanlardir; tanim yeri de o altyapinin kayit noktasidir.
//
// BURADA YALNIZCA TABLO ADI VE SEMA EZILIR. Cerceve tabloyu kendi on eki ve semasiyla
// yazar, bizim semamiza gelmez. Model kurulumunda once cercevenin kurulum metodu, sonra bu
// konfigurasyon calisir; son yazan kazandigi icin tablo modul semasina cekilmis olur.
// Bu siranin sahibi CoreExtensionsDbContextModelCreatingExtensions'tir.
public sealed class EventInboxConfiguration : IEntityTypeConfiguration<IncomingEventRecord>
{
    // Gelen olay kutusunun tablo adini ve semasini uygular; kolon ve indeks tanimina girmez.
    public void Configure(EntityTypeBuilder<IncomingEventRecord> builder)
    {
        builder.ToTable(
            EventBoxConsts.InboxTableName,
            CoreExtensionsDbProperties.DbSchema);
    }
}
