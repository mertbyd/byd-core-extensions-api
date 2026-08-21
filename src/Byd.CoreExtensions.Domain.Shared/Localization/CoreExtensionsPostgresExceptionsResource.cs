using Volo.Abp.Localization;

namespace Byd.CoreExtensions.Localization;

/// <summary>
/// Postgres kaynakli veritabani hatalarinin lokalize mesajlarini tasiyan ayri localization kaynagi.
/// </summary>
// islevi: Postgres SQLSTATE kodlarina karsilik gelen kullanici mesajlarini kendi kaynaginda toplar.
// sistemdeki gorevi: Modulun genel kaynagini saglayici hatalariyla kirletmemek icin ikinci kaynak olarak kayitlidir; PostgresExceptionLocalizationKeyProvider bu kaynak uzerinden cozum yapar.
[LocalizationResourceName(CoreExtensionsLocalizationConsts.PostgresExceptions.ResourceName)]
public class CoreExtensionsPostgresExceptionsResource
{
}
