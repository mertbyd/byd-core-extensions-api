namespace Byd.CoreExtensions.Consts;

/// <summary>
/// Modulun veritabani sema adlarini tanimlar.
/// </summary>
// islevi: Modulun kendi tablolarinin ve ayri tutulan framework tablolarinin sema adlarini derleme guvenli tek sabit kumesinde tutar.
// sistemdeki gorevi: Sema adlarinin tek kaynagidir; CoreExtensionsDbProperties.DbSchema, EF konfigurasyonlari ve
// migration'lar ayni sabitleri okur. Modul bir ana uygulamaya baglandiginda hicbir tablo varsayilan public semasini kirletmez.
public static class DatabaseConsts
{
    // Modulun kendi tablolarinin tamaminin yazildigi sema.
    public const string ModuleSchema = "core_extension";

    // ABP framework tablolarinin (ayar, izin, denetim vb.) yazildigi sema.
    public const string AbpSchema = "abp";
}
