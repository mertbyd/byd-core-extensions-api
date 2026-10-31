using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Byd.CoreExtensions.Entities.EmailTemplates;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// EmailTemplate extended repository sozlesmesini EF Core DbContext'e baglar.
/// </summary>
// islevi: EmailTemplate icin generic, bulk ve DataTable repository yeteneklerini saglar; ayrica kapsam zinciri sorgusunu yazar.
// sistemdeki gorevi: IEmailTemplateRepository sozlesmesinin EfExtendedRepository tabanli provider uygulamasidir.
// mirastan gelen:
//   EfExtendedRepository -> FindAsync, GetListAsync, GetQueryableAsync (ayrica metot yazilmaz)
// burada yazilan: WithDetailsAsync, FindActiveForScopeAsync
[ExposeServices(typeof(IEmailTemplateRepository))]
public class EfCoreEmailTemplateRepository
    : EfExtendedRepository<CoreExtensionsDbContext, EmailTemplate, Guid>,
        IEmailTemplateRepository
{
    // DbContext provider'ini extended repository tabanina devreder.
    public EfCoreEmailTemplateRepository(
        IDbContextProvider<CoreExtensionsDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    // Sablon okuma sorgularina DTO'nun EmailEventName kaynagini ekler.
    public override async Task<IQueryable<EmailTemplate>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(entity => entity.EmailEvent);
    }

    // Organizasyon, ust organizasyon, tenant ve host satirlarini tek sorguda alir ve en yakin kapsami tercih eder.
    // Iki ayri sorgu yerine siralama kullanilir; boylece ust kapsama dusme karari veritabaninda verilir.
    public virtual async Task<EmailTemplate?> FindActiveForScopeAsync(
        Guid emailEventId,
        string cultureName,
        Guid? tenantId,
        Guid? organizationId,
        Guid? parentOrganizationId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query
            .Where(template => template.EmailEventId == emailEventId &&
                               template.CultureName == cultureName &&
                               template.IsActive &&
                               ((template.TenantId == tenantId &&
                                 (template.OrganizationId == null ||
                                  template.OrganizationId == organizationId ||
                                  template.OrganizationId == parentOrganizationId)) ||
                                (template.TenantId == null && template.OrganizationId == null)))
            .OrderByDescending(template => template.OrganizationId != null && template.OrganizationId == organizationId)
            .ThenByDescending(template => template.OrganizationId != null)
            .ThenByDescending(template => template.TenantId != null)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
