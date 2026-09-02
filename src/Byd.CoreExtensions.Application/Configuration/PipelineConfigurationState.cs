using Volo.Abp.DependencyInjection;

namespace Byd.CoreExtensions;

/// <summary>
/// Host HTTP pipeline yapilandirmasinin tamamlandigini tutan bellek durumu.
/// </summary>
// islevi: Host'un UseCoreExtensions cagrisi yaptigini isaretler.
// sistemdeki gorevi: Application modulunun web host pipeline el sikismasini acilista dogrulamasini saglar.
public class PipelineConfigurationState : ISingletonDependency
{
    private volatile bool _isConfigured;

    // Host pipeline el sikismasinin tamamlanip tamamlanmadigini bildirir.
    public bool IsConfigured => _isConfigured;

    // Host pipeline el sikismasini tamamlanmis olarak isaretler.
    public void MarkConfigured()
    {
        _isConfigured = true;
    }
}
