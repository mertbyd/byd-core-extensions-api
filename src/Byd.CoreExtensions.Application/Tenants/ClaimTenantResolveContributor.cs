using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Byd.CoreExtensions.Consts;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Byd.CoreExtensions.Tenants;

/// <summary>
/// Istegin tenant kapsamini token'daki claim'den cozer.
/// </summary>
// islevi: Token'daki tenant claim'ini cercevenin tenant cozumleme zincirine verir.
// sistemdeki gorevi: Tenant'in var ve aktif oldugunu cerceve dogrular; kapsam yetki kontrolunden once kurulur.
public class ClaimTenantResolveContributor : TenantResolveContributorBase
{
    public const string ContributorName = "Claim";

    public override string Name => ContributorName;

    // Claim yoksa zincir cercevenin kendi cozumleyicilerine devam eder.
    public override Task ResolveAsync(ITenantResolveContext context)
    {
        var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();
        var tenantId = currentUser.FindClaimValue(TenantClaimTypes.TenantId);
        if (!string.IsNullOrEmpty(tenantId))
            context.TenantIdOrName = tenantId;

        return Task.CompletedTask;
    }
}
