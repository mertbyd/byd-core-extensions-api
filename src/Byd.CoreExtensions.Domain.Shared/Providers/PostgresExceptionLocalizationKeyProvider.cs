using Microsoft.Extensions.Localization;
using Byd.CoreExtensions.Localization;
using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions.Providers;

/// <summary>
/// Postgres hata kodunu lokalize kullanici mesajina ceviren saglayici.
/// </summary>
// islevi: Verilen SQLSTATE kodunu Postgres hata kaynagindan okuyup lokalize metne donusturur.
// sistemdeki gorevi: Saglayici hatalarinin kullaniciya donmeden once tek noktada metinlestirildigi yer; cagiran katman SQLSTATE bilgisini tasimaz.
public class PostgresExceptionLocalizationKeyProvider : ITransientDependency
{
    private readonly IStringLocalizer<CoreExtensionsPostgresExceptionsResource> _localizer;

    public PostgresExceptionLocalizationKeyProvider(
        IStringLocalizer<CoreExtensionsPostgresExceptionsResource> localizer)
    {
        _localizer = localizer;
    }

    // Hata kodunu kaynaktaki karsiligiyla degistirir; kayit yoksa ABP anahtarin kendisini dondurur.
    public virtual string GetLocalizedExceptionMessage(string errorCode)
    {
        return _localizer[errorCode];
    }
}
