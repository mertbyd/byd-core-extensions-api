namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Cok sutunlu siralamanin tek bir adimi.
/// </summary>
// islevi: Bir siralama adiminin hangi alan uzerinde ve hangi yonde uygulanacagini tasir.
// sistemdeki gorevi: Siralama listesinin ogesi; alan adi dogrudan sorguya gecmez, once izin listesi uzerinden entity yoluna cevrilir.
public class DataTableSorting
{
    // Siralanacak alanin disariya acilan adi; entity property adi degildir.
    public string Field { get; set; } = default!;
    // Azalan siralama istenip istenmedigi; varsayilan artan siralamadir.
    public bool Descending { get; set; }
}
