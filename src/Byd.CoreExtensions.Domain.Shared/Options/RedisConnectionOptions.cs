namespace Byd.CoreExtensions.Options;
/// <summary>
/// Modulun paylasilan Redis baglantisinin secenekleri.
/// </summary>
// islevi: Redis'in acik olup olmadigini ve baglanti adresini tasir.
// sistemdeki gorevi: Modul degerleri host'un Redis bolumunden okur; baska bir kaynak kullanmak isteyen host Configure ile ezer.
public class RedisConnectionOptions
{
    // Redis baglantisinin acik olup olmadigi; beyan edilmezse kapalidir.
    public bool IsEnabled { get; set; }
    // StackExchange.Redis baglanti adresi; ornek "127.0.0.1:6379".
    public string? Configuration { get; set; }
}
