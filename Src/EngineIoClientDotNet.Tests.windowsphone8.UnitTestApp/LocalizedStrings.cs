using EngineIoClientDotNet.Tests.windowsphone8.UnitTestApp.Resources;

namespace EngineIoClientDotNet.Tests.windowsphone8.UnitTestApp
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}