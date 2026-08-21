using Volo.Abp.Modularity;

namespace Byd.CoreExtensions;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class CoreExtensionsApplicationTestBase<TStartupModule> : CoreExtensionsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
