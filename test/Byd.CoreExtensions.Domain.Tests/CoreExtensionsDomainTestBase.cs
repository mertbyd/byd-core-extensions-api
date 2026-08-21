using Volo.Abp.Modularity;

namespace Byd.CoreExtensions;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class CoreExtensionsDomainTestBase<TStartupModule> : CoreExtensionsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
