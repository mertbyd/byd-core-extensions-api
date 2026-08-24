using System.Collections.Generic;

namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Tek bir alan icin gonderilen filtre istegi.
/// </summary>
// islevi: Bir filtre alanina uygulanacak operator belirtecini ve ham metin deger(ler)ini tasir.
// sistemdeki gorevi: Filtre modelindeki her alanin tipi; motor bu ogeden tek bir Expression parcasi uretir.
public class DataTableFilterItem
{
    // Uygulanacak karsilastirmanin belirteci; gecerli degerler DataTableFilterOperator sabitleridir.
    // Bos gelirse motor istegi belirsiz sayarak reddeder, varsayilan bir operator secmez.
    public string? Operator { get; set; }

    // Karsilastirmanin ham metin degeri; hedef tipe donusum motorun isidir.
    public string? Value { get; set; }

    // Araligin ust siniri; yalniz Between operatorunde anlamlidir.
    public string? SecondValue { get; set; }

    // Coklu deger filtresinin ham metin listesi; yalniz In operatorunde anlamlidir.
    public List<string>? Values { get; set; }
}
