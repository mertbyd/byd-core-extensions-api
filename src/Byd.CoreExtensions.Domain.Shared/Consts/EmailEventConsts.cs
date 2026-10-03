namespace Byd.CoreExtensions.Consts;

/// <summary>
/// E-posta olayi lookup tablosu, indeks ve alan siniri degerlerini tanimlar.
/// </summary>
// islevi: EmailEvent kalici modeli ile EF konfigurasyonunun ortak sabitlerini tek yerde tutar.
// sistemdeki gorevi: Domain ve EF konfigurasyonunun ayni kararli sinirlari kullanmasini saglar. Olay listesinin sahibi
// olayi gonderen moduldur; core hicbir olay adi ya da kimligi beyan etmez.
public static class EmailEventConsts
{
    public const string TableName = "EmailEvents";
    public const string NameIndex = "IX_EmailEvents_Name";
    public const int MaxNameLength = 128;
}
