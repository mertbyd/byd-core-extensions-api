namespace Byd.CoreExtensions.Querying;

/// <summary>
/// Filtre degerini tipli bir kapsayici uzerinde tutarak EF Core'un sabit yerine parametre uretmesini saglar.
/// </summary>
// islevi: Tek bir filtre degerini tipli bir property arkasinda saklar.
// sistemdeki gorevi: QueryableExtensions'in kurdugu ifade agacinda deger, sabit bir kapsayici uzerindeki uye erisimi olur; EF Core bunu yakalanmis parametre gibi gorup SQL parametresine cevirir ve boylece plan cache korunur.
public sealed class QueryValueBox<T>
{
    public QueryValueBox(T value)
    {
        Value = value;
    }

    // Sorguya parametre olarak gecen filtre degeri.
    public T Value { get; }
}
