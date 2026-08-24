namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Sinifin cache icin hazirlanmis projeksiyon oldugunu isaretleyen bos sozlesme.
/// </summary>
// islevi: Tipin cache'e yazilmak uzere tasarlanmis sade bir projeksiyon oldugunu bildirir; uye tasimaz ve kimlik saglamaz.
// sistemdeki gorevi: Cache'li repository ve domain servis ailesinin jenerik kisit hedefi; kimlik ayrica IEntity<TKey> uzerinden gelir, boylece anahtar tipi sozlesmeye gomulmez.
public interface ICacheModel
{
}
