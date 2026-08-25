namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// Modelin aktif/pasif durumu tasidigini garanti eden sozlesme.
/// </summary>
// islevi: Tipin IsActive alanina sahip oldugunu bildirir; jenerik kod somut tipi bilmeden aktiflik okuyup yazabilir.
// sistemdeki gorevi: Global aktiflik filtresinin ve jenerik repository/manager kisitlarinin baglandigi ortak nokta.
public interface IHasIsActive
{
    // Kaydin aktif olup olmadigi; global aktiflik filtresi bu alani okur.
    bool IsActive { get; set; }
}
