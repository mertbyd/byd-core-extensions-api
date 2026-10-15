using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Volo.Abp.Http;

namespace Byd.CoreExtensions.HealthChecks;

/// <summary>
/// Saglik raporunu JSON olarak yazar.
/// </summary>
// islevi: Genel durumu ve her yoklamanin durumunu, aciklamasini ve suresini tek JSON govdesinde doner.
// sistemdeki gorevi: Izleme araclari ve operasyon ekrani hangi bagimliligin bozuk oldugunu tek istekte gorur; hata ayrintisi
// govdeye yazilmaz.
public static class HealthCheckResponseWriter
{
    // Raporu durum, toplam sure ve yoklama listesiyle yazar.
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = MimeTypes.Application.Json;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Status = report.Status.ToString(),
            DurationMilliseconds = report.TotalDuration.TotalMilliseconds,
            Entries = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                entry.Value.Description,
                DurationMilliseconds = entry.Value.Duration.TotalMilliseconds
            })
        }, JsonSerializerOptions.Web));
    }
}
