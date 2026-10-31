namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Entity degistiginde onbellekteki yanitlarin temizlenecegini isaretleyen bos sozlesme.
/// </summary>
// islevi: Tipin, adiyla ayni gruptaki onbellekli yanitlarin kaynagi oldugunu bildirir; uye tasimaz.
// sistemdeki gorevi: BydDbContext kaydetmede isaretli entity'leri toplar ve gruplarinin temizlenmesini ister; yanit onbellegi
// boylece veri degisir degismez tazelenir, yalniz sureye kalmaz.
public interface ICachedResponseSource
{
}
