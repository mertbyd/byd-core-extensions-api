using System;
using System.Threading.Tasks;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Interfaces;
using Shouldly;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// Yanit onbellegi interceptor'unun servis metodunu Redis uzerinden sakladigini dogrular.
/// </summary>
// islevi: Isaretli metodun ikinci cagrida calismadan dondugunu, girdisi ya da tenant'i farkli cagrinin ayri saklandigini,
// grup temizlenince yeniden calistigini, isaretsiz metodun hic saklanmadigini, taban siniftaki isaretin turetilmis serviste
// calistigini, servisin kendi icinden yaptigi cagrinin saklanmadigini ve temizlik isaretli yazmanin grubu yalniz basariyla
// bittiginde sildigini kanitlar.
// sistemdeki gorevi: Attribute'un AppService metodunda ve CRUD tabaninda calistiginin guvencesidir.
public class CachedResponseInterceptor_Tests : CoreExtensionsEntityFrameworkCoreTestBase
{
    private readonly ICachedProbeAppService _service;

    public CachedResponseInterceptor_Tests()
    {
        _service = GetRequiredService<ICachedProbeAppService>();
    }

    [RedisFact]
    public async Task Marked_Method_Is_Served_From_Cache_Until_Its_Group_Is_Removed()
    {
        var id = Guid.NewGuid();
        var first = await _service.GetAsync(id);

        (await _service.GetAsync(id)).CallNumber.ShouldBe(first.CallNumber);
        (await _service.GetAsync(Guid.NewGuid())).CallNumber.ShouldNotBe(first.CallNumber);
        using (GetRequiredService<ICurrentTenant>().Change(Guid.NewGuid()))
            (await _service.GetAsync(id)).CallNumber.ShouldNotBe(first.CallNumber);

        await GetRequiredService<ICachedResponseStore>().RemoveGroupAsync(CachedProbeAppService.Group);
        (await _service.GetAsync(id)).CallNumber.ShouldBeGreaterThan(first.CallNumber);
    }

    [RedisFact]
    public async Task Clearing_Method_Removes_Its_Group_After_It_Completes()
    {
        var id = Guid.NewGuid();
        var first = await _service.GetAsync(id);

        await _service.ChangeAsync();

        (await _service.GetAsync(id)).CallNumber.ShouldBeGreaterThan(first.CallNumber);
    }

    [RedisFact]
    public async Task Failing_Clearing_Method_Keeps_Its_Group()
    {
        var id = Guid.NewGuid();
        var first = await _service.GetAsync(id);

        await Should.ThrowAsync<InvalidOperationException>(() => _service.ChangeAndFailAsync());

        (await _service.GetAsync(id)).CallNumber.ShouldBe(first.CallNumber);
    }

    [RedisFact]
    public async Task Unmarked_Method_Always_Runs()
    {
        var id = Guid.NewGuid();
        var first = await _service.GetUncachedAsync(id);

        (await _service.GetUncachedAsync(id)).CallNumber.ShouldBeGreaterThan(first.CallNumber);
    }

    [RedisFact]
    public async Task Marked_Method_Of_Base_Class_Is_Cached_For_Derived_Service()
    {
        var id = Guid.NewGuid();
        var first = await _service.GetFromBaseAsync(id);

        (await _service.GetFromBaseAsync(id)).CallNumber.ShouldBe(first.CallNumber);
    }

    [RedisFact]
    public async Task Marked_Method_Called_From_Inside_The_Service_Is_Not_Cached()
    {
        var id = Guid.NewGuid();
        var first = await _service.GetThroughInnerCallAsync(id);

        (await _service.GetThroughInnerCallAsync(id)).CallNumber.ShouldBeGreaterThan(first.CallNumber);
    }
}
