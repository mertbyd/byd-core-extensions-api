using System.Threading;
using Byd.CoreExtensions.Interfaces;
using Byd.CoreExtensions.Models.Organizations;

namespace Byd.CoreExtensions.Providers;

/// <summary>
/// Ustelenmis organizasyon kapsamini cagri zinciri boyunca tasiyan saklama noktasi.
/// </summary>
// islevi: Kapsami AsyncLocal icinde tutar; yazilan deger yalnizca onu yazan cagri zincirinde ve altinda gorunur.
// sistemdeki gorevi: ICurrentOrganizationAccessor'in tekil uygulamasi. AsyncLocal secildi cunku kapsam await
// sinirlarini asmali, ayni anda calisan baska bir istegin kapsamina sizmamalidir. Tekilligi kendi ornegiyle
// tasir; boylece DI disindan da ayni kapsam okunabilir ve tenant tarafindaki karsiligiyla ayni sekli korur.
public class AsyncLocalCurrentOrganizationAccessor : ICurrentOrganizationAccessor
{
    private readonly AsyncLocal<BasicOrganizationInfo?> _currentScope;

    // Modulun kaydettigi ve tum cagri zincirlerinin paylastigi tek ornek.
    public static AsyncLocalCurrentOrganizationAccessor Instance { get; } = new();

    // Yururlukteki ustelenmis kapsam; yoksa null.
    public BasicOrganizationInfo? Current
    {
        get => _currentScope.Value;
        set => _currentScope.Value = value;
    }

    private AsyncLocalCurrentOrganizationAccessor()
    {
        _currentScope = new AsyncLocal<BasicOrganizationInfo?>();
    }
}
