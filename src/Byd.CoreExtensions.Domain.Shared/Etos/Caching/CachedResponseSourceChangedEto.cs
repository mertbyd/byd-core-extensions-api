using System;

namespace Byd.CoreExtensions.Etos.Caching;

/// <summary>
/// Onbellekli yanitlarin kaynagi olan bir entity tipinin degistigini bildiren yerel olay.
/// </summary>
// islevi: Temizlenecek grubun adini ve degisikligin yapildigi tenant'i tasir.
// sistemdeki gorevi: Kaydetme aninda isaretli entity'leri toplayan DbContext ile yanit grubunu silen handler arasindaki surec ici
// sozlesmedir. Dagitik degildir; commit tamamlaninca yayinlanir.
public sealed record CachedResponseSourceChangedEto(string Group, Guid? TenantId);
