using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Byd.CoreExtensions.Consts;
using Byd.CoreExtensions.Interfaces;
using Volo.Abp.AspNetCore.Middleware;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace Byd.CoreExtensions.Organizations;

/// <summary>
/// Istegin organizasyon kapsamini token'daki claim'lerden kurar.
/// </summary>
// islevi: Kimlik dogrulanmissa organizasyon ve ust organizasyon claim'lerini okur ve istegin kalanini o kapsamda
// yurutur.
// sistemdeki gorevi: Kapsamin istek yolundaki tek kurulma noktasi. ICurrentOrganization yalnizca okur; kapsami
// buraya koymak, downstream kodun claim ayristirmasini tekrar etmesini ve her tuketicinin kendi kuralini
// uydurmasini engeller.
// Taban ABP'nin middleware tabanidir: tenant tarafindaki karsiligi da ayni tabandan turer ve konvansiyonel
// RequestDelegate imzasi yerine IMiddleware kullanir, boylece istek basina degisen bagimliliklar alan olarak
// tutulmak zorunda kalmaz.
public class OrganizationMiddleware(
    ICurrentUser currentUser,
    ICurrentOrganization currentOrganization) : AbpMiddlewareBase, ITransientDependency
{
    // Kimlik yoksa veya claim gecerli bir kimlik tasimiyorsa kapsam acilmaz ve istek dokunulmadan gecer;
    // adi olup kimligi olmayan bir kapsam hicbir sorguyu daraltamaz, yalnizca yaniltir.
    public override async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!currentUser.IsAuthenticated
            || !Guid.TryParse(currentUser.FindClaimValue(OrganizationClaimTypes.OrganizationId), out var organizationId))
        {
            await next(context);
            return;
        }
        // Ust organizasyon claim'i yalniz alt organizasyon seciminde gelir; yoksa kapsam kok organizasyondur.
        Guid? parentId = Guid.TryParse(currentUser.FindClaimValue(OrganizationClaimTypes.ParentId), out var parsedParentId)
            ? parsedParentId
            : null;

        using (currentOrganization.Change(
                   organizationId,
                   currentUser.FindClaimValue(OrganizationClaimTypes.OrganizationName),
                   parentId,
                   currentUser.FindClaimValue(OrganizationClaimTypes.ParentName)))
        {
            await next(context);
        }
    }
}
