using System;
using System.Threading;
using System.Threading.Tasks;
using Byd.CoreExtensions.Entities.EmailTemplates;

namespace Byd.CoreExtensions.Interfaces;

/// <summary>
/// E-posta sablonu aggregate'i icin extended repository sozlesmesini tanimlar.
/// </summary>
// islevi: EmailTemplate CRUD ve kapsam zinciri sorgusunu Domain katmanina soyutlar.
// sistemdeki gorevi: EmailTemplateManager ile EF Core repository uygulamasi arasindaki veri erisim siniridir.
public interface IEmailTemplateRepository : IExtendedRepository<EmailTemplate>
{
    // Olayin verilen dildeki aktif satirini en yakin kapsamdan getirir: organizasyon, ust organizasyon, tenant, host.
    // Hicbir kapsamda aktif satir yoksa null doner.
    Task<EmailTemplate?> FindActiveForScopeAsync(
        Guid emailEventId,
        string cultureName,
        Guid? tenantId,
        Guid? organizationId,
        Guid? parentOrganizationId,
        CancellationToken cancellationToken = default);
}
