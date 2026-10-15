using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Byd.CoreExtensions.Attributes;
using Byd.CoreExtensions.EntityFrameworkCore;
using Byd.CoreExtensions.ExceptionCodes;
using Byd.CoreExtensions.Providers;
using Byd.CoreExtensions.Querying;
using Byd.CoreExtensions.Repositories;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace Byd.CoreExtensions.Elasticsearch;

public enum SearchProbeStatus
{
    Draft = 1,
    Active = 2,
    Closed = 3
}

// Evdeki entity sekli: korumali parametresiz ctor, internal setter.
public class SearchProbe : Entity<Guid>
{
    protected SearchProbe()
    {
    }

    public SearchProbe(Guid id, string name, SearchProbeStatus status, int score, double amount, DateTime createdAt, string? note)
        : base(id)
    {
        Name = name;
        Status = status;
        Score = score;
        Amount = amount;
        CreatedAt = createdAt;
        Note = note;
    }

    public string Name { get; internal set; } = default!;
    public SearchProbeStatus Status { get; internal set; }
    public int Score { get; internal set; }
    public double Amount { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public string? Note { get; internal set; }
}

public class SearchProbeFilter
{
    [DataTable("id", nameof(SearchProbe.Id))]
    public DataTableFilterItem? Id { get; set; }
    [DataTable("name", nameof(SearchProbe.Name))]
    public DataTableFilterItem? Name { get; set; }
    [DataTable("status", nameof(SearchProbe.Status))]
    public DataTableFilterItem? Status { get; set; }
    [DataTable("score", nameof(SearchProbe.Score))]
    public DataTableFilterItem? Score { get; set; }
    [DataTable("amount", nameof(SearchProbe.Amount))]
    public DataTableFilterItem? Amount { get; set; }
    [DataTable("createdAt", nameof(SearchProbe.CreatedAt))]
    public DataTableFilterItem? CreatedAt { get; set; }
    [DataTable("note", nameof(SearchProbe.Note))]
    public DataTableFilterItem? Note { get; set; }
}

public class SearchProbeRepository : ElasticsearchRepository<SearchProbe, Guid>
{
    public static string Index { get; set; } = default!;
    protected override string IndexName => Index;
}

// Ayni indekse Score alanini metin olarak yazan belge; esleme catismasi uretmek icin.
public class ConflictingProbe : Entity<Guid>
{
    public ConflictingProbe(Guid id, string score) : base(id)
    {
        Score = score;
    }

    public string Score { get; set; }
}

public class ConflictingProbeRepository : ElasticsearchRepository<ConflictingProbe, Guid>
{
    protected override string IndexName => SearchProbeRepository.Index;
}

// Tenant'a bagli belge; okumalar yururlukteki tenant'la sinirlanmali.
public class ScopedProbe : Entity<Guid>, IMultiTenant
{
    protected ScopedProbe()
    {
    }

    public ScopedProbe(Guid id, Guid? tenantId, string name) : base(id)
    {
        TenantId = tenantId;
        Name = name;
    }

    public Guid? TenantId { get; internal set; }
    public string Name { get; internal set; } = default!;
}

public class ScopedProbeRepository : ElasticsearchRepository<ScopedProbe, Guid>
{
    public const string IndexSuffix = "-scoped";
    protected override string IndexName => SearchProbeRepository.Index + IndexSuffix;
}

public class ElasticsearchRepository_Tests : CoreExtensionsEntityFrameworkCoreTestBase, IAsyncLifetime
{
    private static readonly DateTime BaseTime = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
    private readonly SearchProbeRepository _repository;

    public ElasticsearchRepository_Tests()
    {
        SearchProbeRepository.Index = $"core-search-probe-{Guid.NewGuid():N}";
        _repository = GetRequiredService<SearchProbeRepository>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ElasticsearchFactAttribute.UrlVariable)))
            return;
        var client = await GetRequiredService<ElasticsearchClientProvider>().GetClientAsync();
        await client.Indices.DeleteAsync(SearchProbeRepository.Index, r => r.IgnoreUnavailable());
        await client.Indices.DeleteAsync(SearchProbeRepository.Index + ScopedProbeRepository.IndexSuffix, r => r.IgnoreUnavailable());
    }

    private static List<SearchProbe> Seed() =>
    [
        new(Guid.NewGuid(), "alpha station", SearchProbeStatus.Active, 10, 1.5, BaseTime, null),
        new(Guid.NewGuid(), "beta stop*", SearchProbeStatus.Draft, 20, 2.5, BaseTime.AddDays(1), "has note"),
        new(Guid.NewGuid(), "gamma terminal", SearchProbeStatus.Closed, 30, 3.5, BaseTime.AddDays(2), "second"),
        new(Guid.NewGuid(), "delta station", SearchProbeStatus.Active, 40, 4.5, BaseTime.AddDays(3), null),
        new(Guid.NewGuid(), "epsilon", SearchProbeStatus.Active, 50, 5.5, BaseTime.AddDays(4), "third"),
    ];

    private async Task<List<SearchProbe>> SeedAsync()
    {
        var probes = Seed();
        await _repository.InsertManyAsync(probes);
        return probes;
    }

    private static DataTableListInput<SearchProbeFilter> Input(SearchProbeFilter? filters = null, int page = 1, int pageSize = 10, params DataTableSorting[] sorting) =>
        new() { Filters = filters, Page = page, PageSize = pageSize, Sorting = sorting.ToList() };

    private static DataTableFilterItem F(string op, string? value = null, string? second = null, List<string>? values = null) =>
        new() { Operator = op, Value = value, SecondValue = second, Values = values };

    private async Task<List<string>> NamesAsync(SearchProbeFilter filters)
    {
        var input = Input(filters, sorting: new DataTableSorting { Field = "score" });
        var list = await _repository.GetFilteredListAsync(input);
        var count = await _repository.GetFilteredCountAsync(input);
        count.ShouldBe(list.Count);
        return list.Select(x => x.Name).ToList();
    }

    [ElasticsearchFact]
    public async Task Missing_Index_Returns_Empty_Results()
    {
        (await _repository.IndexExistsAsync()).ShouldBeFalse();
        (await _repository.GetFilteredListAsync(Input())).ShouldBeEmpty();
        (await _repository.GetFilteredCountAsync(Input())).ShouldBe(0);
        (await _repository.GetListAsync()).ShouldBeEmpty();
        (await _repository.GetCountAsync()).ShouldBe(0);
        (await _repository.FindAsync(Guid.NewGuid())).ShouldBeNull();
    }

    [ElasticsearchFact]
    public async Task Index_Management_Creates_Clears_And_Pings()
    {
        (await _repository.CheckConnectionAsync()).ShouldBeTrue();
        (await _repository.CreateIndexAsync()).ShouldBeTrue();
        (await _repository.IndexExistsAsync()).ShouldBeTrue();

        await SeedAsync();
        (await _repository.GetCountAsync()).ShouldBe(5);

        await _repository.ClearIndexAsync();
        (await _repository.GetCountAsync()).ShouldBe(0);
        (await _repository.IndexExistsAsync()).ShouldBeTrue();
    }

    [ElasticsearchFact]
    public async Task Insert_And_Find_Round_Trips_Entity_Shaped_Document()
    {
        var probe = new SearchProbe(Guid.NewGuid(), "alpha", SearchProbeStatus.Closed, 7, 1.25, BaseTime, "note");
        await _repository.InsertAsync(probe);

        var found = await _repository.FindAsync(probe.Id);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(probe.Id);
        found.Name.ShouldBe("alpha");
        found.Status.ShouldBe(SearchProbeStatus.Closed);
        found.Score.ShouldBe(7);
        found.Amount.ShouldBe(1.25);
        found.CreatedAt.ShouldBe(BaseTime);
        found.Note.ShouldBe("note");
        (await _repository.FindAsync(Guid.NewGuid())).ShouldBeNull();
    }

    [ElasticsearchFact]
    public async Task Every_Operator_Filters_In_The_Index()
    {
        var probes = await SeedAsync();

        (await NamesAsync(new() { Name = F(DataTableFilterOperator.Equal, "epsilon") })).ShouldBe(["epsilon"]);
        (await NamesAsync(new() { Status = F(DataTableFilterOperator.NotEqual, "Active") })).ShouldBe(["beta stop*", "gamma terminal"]);
        (await NamesAsync(new() { Score = F(DataTableFilterOperator.GreaterThan, "30") })).ShouldBe(["delta station", "epsilon"]);
        (await NamesAsync(new() { Score = F(DataTableFilterOperator.GreaterThanOrEqual, "30") })).Count.ShouldBe(3);
        (await NamesAsync(new() { Amount = F(DataTableFilterOperator.LessThan, "2.5") })).ShouldBe(["alpha station"]);
        (await NamesAsync(new() { Amount = F(DataTableFilterOperator.LessThanOrEqual, "2.5") })).Count.ShouldBe(2);
        (await NamesAsync(new() { Name = F(DataTableFilterOperator.Contains, "STATION") })).ShouldBe(["alpha station", "delta station"]);
        (await NamesAsync(new() { Name = F(DataTableFilterOperator.Contains, "stop*") })).ShouldBe(["beta stop*"]);
        (await NamesAsync(new() { Name = F(DataTableFilterOperator.NotContains, "station") })).ShouldBe(["beta stop*", "gamma terminal", "epsilon"]);
        (await NamesAsync(new() { Name = F(DataTableFilterOperator.Match, "Gamma Terminal") })).ShouldBe(["gamma terminal"]);
        (await NamesAsync(new() { Name = F(DataTableFilterOperator.Fuzzy, "statoin") })).ShouldBe(["alpha station", "delta station"]);
        (await NamesAsync(new() { CreatedAt = F(DataTableFilterOperator.Between, BaseTime.AddDays(1).ToString("O"), BaseTime.AddDays(2).ToString("O")) }))
            .ShouldBe(["beta stop*", "gamma terminal"]);
        (await NamesAsync(new() { Status = F(DataTableFilterOperator.In, values: ["Draft", "Closed"]) })).ShouldBe(["beta stop*", "gamma terminal"]);
        (await NamesAsync(new() { Note = F(DataTableFilterOperator.IsNull) })).ShouldBe(["alpha station", "delta station"]);
        (await NamesAsync(new() { Note = F(DataTableFilterOperator.IsNotNull) })).Count.ShouldBe(3);
        (await NamesAsync(new() { Id = F(DataTableFilterOperator.Equal, probes[2].Id.ToString()) })).ShouldBe(["gamma terminal"]);
        (await NamesAsync(new() { Status = F(DataTableFilterOperator.Equal, "Active"), Score = F(DataTableFilterOperator.LessThan, "45") }))
            .ShouldBe(["alpha station", "delta station"]);
    }

    [ElasticsearchFact]
    public async Task Sorting_And_Paging_Follow_The_Request()
    {
        await SeedAsync();

        var page = await _repository.GetFilteredListAsync(Input(page: 2, pageSize: 2, sorting: new DataTableSorting { Field = "score", Descending = true }));
        page.Select(x => x.Score).ShouldBe([30, 20]);

        var byName = await _repository.GetFilteredListAsync(Input(sorting: new DataTableSorting { Field = "name" }));
        byName.Select(x => x.Name).ShouldBe(byName.Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal).ToList());

        var unknownField = await _repository.GetFilteredListAsync(Input(sorting: new DataTableSorting { Field = "notAllowed" }));
        unknownField.Count.ShouldBe(5);
        (await _repository.GetFilteredListAsync(Input(pageSize: 2), usePaging: false)).Count.ShouldBe(5);
    }

    [ElasticsearchFact]
    public async Task Search_Returns_Page_And_Total_And_Cached_Search_Survives_Writes()
    {
        await SeedAsync();
        var input = Input(new SearchProbeFilter { Status = F(DataTableFilterOperator.Equal, "Active") }, pageSize: 2,
            sorting: new DataTableSorting { Field = "score" });

        var result = await _repository.SearchAsync(input);
        result.Items.Select(x => x.Name).ShouldBe(["alpha station", "delta station"]);
        result.TotalCount.ShouldBe(3);

        var cached = await _repository.CachedSearchAsync(input);
        await _repository.ClearIndexAsync();
        var cachedAgain = await _repository.CachedSearchAsync(input);
        cachedAgain.Items.Select(x => x.Id).ShouldBe(cached.Items.Select(x => x.Id));
        cachedAgain.TotalCount.ShouldBe(3);
        (await _repository.SearchAsync(input)).TotalCount.ShouldBe(0);
    }

    [ElasticsearchFact]
    public async Task Field_Date_Range_All_And_Any_Reads()
    {
        var probes = await SeedAsync();

        (await _repository.GetListByAsync(probe => probe.Status, SearchProbeStatus.Active)).Count.ShouldBe(3);
        (await _repository.GetListByAsync(probe => probe.Name, "epsilon")).Single().Id.ShouldBe(probes[4].Id);
        (await _repository.GetListByDateRangeAsync(probe => probe.CreatedAt, BaseTime.AddDays(1), BaseTime.AddDays(3)))
            .Select(x => x.Name).OrderBy(x => x).ShouldBe(["beta stop*", "delta station", "gamma terminal"]);
        (await _repository.GetAllListAsync()).Count.ShouldBe(5);
        (await _repository.AnyAsync(new SearchProbeFilter { Score = F(DataTableFilterOperator.GreaterThan, "45") })).ShouldBeTrue();
        (await _repository.AnyAsync(new SearchProbeFilter { Score = F(DataTableFilterOperator.GreaterThan, "99") })).ShouldBeFalse();
    }

    [ElasticsearchFact]
    public async Task Update_And_Delete_Change_The_Stored_Documents()
    {
        var probes = await SeedAsync();
        var target = probes[0];

        await _repository.UpdateAsync(target.Id, new { Name = "renamed", Note = "added" });
        var updated = await _repository.FindAsync(target.Id);
        updated!.Name.ShouldBe("renamed");
        updated.Note.ShouldBe("added");
        updated.Score.ShouldBe(10);

        probes[1].Score = 21;
        probes[2].Score = 31;
        await _repository.UpdateManyAsync([probes[1], probes[2]]);
        (await _repository.FindAsync(probes[2].Id))!.Score.ShouldBe(31);

        await _repository.DeleteAsync(target.Id);
        (await _repository.FindAsync(target.Id)).ShouldBeNull();
        await _repository.DeleteAsync(target.Id);
        await _repository.DeleteManyAsync([probes[1].Id, probes[2].Id, Guid.NewGuid()]);
        (await _repository.GetCountAsync()).ShouldBe(2);
    }

    [ElasticsearchFact]
    public async Task Update_Of_Missing_Document_Throws()
    {
        await SeedAsync();

        await Should.ThrowAsync<TransportException>(() => _repository.UpdateAsync(Guid.NewGuid(), new { Name = "ghost" }));
    }

    [ElasticsearchFact]
    public async Task Invalid_Requests_Raise_The_Same_Coded_Errors_As_The_Database_Path()
    {
        await SeedAsync();
        static SearchProbeFilter WithScore(string op, string value) => new() { Score = new DataTableFilterItem { Operator = op, Value = value } };

        (await Should.ThrowAsync<BusinessException>(() => _repository.GetFilteredListAsync(Input(WithScore(DataTableFilterOperator.Equal, "abc")))))
            .Code.ShouldBe(GeneralExceptionCodes.InvalidFilterValue);
        (await Should.ThrowAsync<BusinessException>(() => _repository.GetFilteredListAsync(Input(WithScore("~", "1")))))
            .Code.ShouldBe(GeneralExceptionCodes.UnsupportedFilterOperator);
        (await Should.ThrowAsync<BusinessException>(() => _repository.GetFilteredListAsync(Input(WithScore(DataTableFilterOperator.Contains, "1")))))
            .Code.ShouldBe(GeneralExceptionCodes.UnsupportedFilterOperator);
        (await Should.ThrowAsync<BusinessException>(() => _repository.GetFilteredListAsync(Input(WithScore(DataTableFilterOperator.Match, "1")))))
            .Code.ShouldBe(GeneralExceptionCodes.UnsupportedFilterOperator);
        (await Should.ThrowAsync<BusinessException>(() => _repository.GetFilteredListAsync(Input(page: 0))))
            .Code.ShouldBe(GeneralExceptionCodes.InvalidPage);
        (await Should.ThrowAsync<BusinessException>(() => _repository.GetFilteredListAsync(Input(pageSize: 1001))))
            .Code.ShouldBe(GeneralExceptionCodes.InvalidPageSize);
    }

    [ElasticsearchFact]
    public async Task Bulk_Write_With_Rejected_Documents_Raises_Coded_Error()
    {
        await SeedAsync();
        var conflicting = GetRequiredService<ConflictingProbeRepository>();

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            conflicting.InsertManyAsync([new ConflictingProbe(Guid.NewGuid(), "not a number"), new ConflictingProbe(Guid.NewGuid(), "12")]));

        exception.Code.ShouldBe(ElasticsearchExceptionCodes.BulkWriteFailed);
        exception.Data["failedItemCount"].ShouldBe(1);
    }

    [ElasticsearchFact]
    public async Task Tenant_Scoped_Documents_Are_Read_Only_In_Their_Tenant()
    {
        var repository = GetRequiredService<ScopedProbeRepository>();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        await repository.InsertManyAsync(
        [
            new ScopedProbe(Guid.NewGuid(), tenantA, "a1"),
            new ScopedProbe(Guid.NewGuid(), tenantA, "a2"),
            new ScopedProbe(Guid.NewGuid(), tenantB, "b1"),
            new ScopedProbe(Guid.NewGuid(), null, "host")
        ]);

        using (GetRequiredService<ICurrentTenant>().Change(tenantA))
            (await repository.GetListAsync()).Select(x => x.Name).OrderBy(x => x).ShouldBe(["a1", "a2"]);
        using (GetRequiredService<ICurrentTenant>().Change(tenantB))
            (await repository.GetCountAsync()).ShouldBe(1);
        (await repository.GetListAsync()).Single().Name.ShouldBe("host");
        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
            (await repository.GetCountAsync()).ShouldBe(4);
    }
}
