using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.Interfaces;
using Shouldly;
using Volo.Abp.MultiTenancy;

namespace Byd.CoreExtensions.Caching;

/// <summary>
/// Yanit onbellegi deposunun Redis uzerindeki okuma, yazma ve grup temizligini dogrular.
/// </summary>
// islevi: Yazilan yanitin ayni kapsamda okundugunu, baska tenant'tan okunmadigini ve grup temizliginin yalniz o grubu
// sildigini kanitlar; dagitik onbellegin Redis'e gectigini de dogrular.
// sistemdeki gorevi: Desenle silmenin dayandigi anahtar biciminin bozulmadiginin guvencesidir.
public class CachedResponseStore_Tests : CoreExtensionsEntityFrameworkCoreTestBase
{
    private readonly CachedResponseStore _store;
    private readonly string _group = $"Probe{Guid.NewGuid():N}";

    public CachedResponseStore_Tests()
    {
        _store = GetRequiredService<CachedResponseStore>();
    }

    [RedisFact]
    public async Task Stored_Response_Is_Read_Back_Only_In_Its_Tenant()
    {
        _store.IsAvailable.ShouldBeTrue();
        GetRequiredService<IDistributedCache>().ShouldBeAssignableTo<RedisCache>();

        await _store.SetAsync(_group, "fingerprint", "{\"value\":1}", TimeSpan.FromMinutes(1));

        (await _store.FindAsync(_group, "fingerprint")).ShouldBe("{\"value\":1}");
        using (GetRequiredService<ICurrentTenant>().Change(Guid.NewGuid()))
            (await _store.FindAsync(_group, "fingerprint")).ShouldBeNull();
    }

    [RedisFact]
    public async Task Removing_A_Group_Removes_Only_That_Group()
    {
        var otherGroup = _group + "Other";
        await _store.SetAsync(_group, "first", "1", TimeSpan.FromMinutes(1));
        await _store.SetAsync(_group, "second", "2", TimeSpan.FromMinutes(1));
        await _store.SetAsync(otherGroup, "first", "3", TimeSpan.FromMinutes(1));

        await GetRequiredService<ICachedResponseStore>().RemoveGroupAsync(_group);

        (await _store.FindAsync(_group, "first")).ShouldBeNull();
        (await _store.FindAsync(_group, "second")).ShouldBeNull();
        (await _store.FindAsync(otherGroup, "first")).ShouldBe("3");
        await _store.RemoveGroupAsync(otherGroup);
    }
}
