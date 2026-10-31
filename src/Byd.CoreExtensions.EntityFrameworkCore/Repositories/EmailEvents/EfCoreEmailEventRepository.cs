using System;
using Byd.CoreExtensions.Entities.EmailEvents;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace Byd.CoreExtensions.EntityFrameworkCore;

/// <summary>
/// EmailEvent extended repository sozlesmesini EF Core DbContext'e baglar.
/// </summary>
// islevi: EmailEvent icin generic, bulk ve DataTable repository yeteneklerini saglar.
// sistemdeki gorevi: IEmailEventRepository sozlesmesinin EfExtendedRepository tabanli provider uygulamasidir.
// mirastan gelen:
//   EfExtendedRepository -> FindAsync, GetListAsync, InsertAsync (ayrica metot yazilmaz)
[ExposeServices(typeof(IEmailEventRepository))]
public class EfCoreEmailEventRepository
    : EfExtendedRepository<CoreExtensionsDbContext, EmailEvent, Guid>,
        IEmailEventRepository
{
    // DbContext provider'ini extended repository tabanina devreder.
    public EfCoreEmailEventRepository(
        IDbContextProvider<CoreExtensionsDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
