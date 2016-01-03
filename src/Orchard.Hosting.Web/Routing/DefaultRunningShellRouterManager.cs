using Microsoft.AspNet.Routing;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.Routing
{
    public class DefaultRunningShellRouterManager : IRunningShellRouterManager, IShellSettingsManagerEventHandler
    {
        private readonly ShellSettings _shellSettings;
        private readonly IRunningShellRouterTable _runningShellRouterTable;
        private readonly IRouteBuilder _routeBuilder;

        public DefaultRunningShellRouterManager(ShellSettings shellSettings,
            IRunningShellRouterTable runningShellRouterTable,
            IRouteBuilder routeBuilder)
        {
            _shellSettings = shellSettings;
            _runningShellRouterTable = runningShellRouterTable;
            _routeBuilder = routeBuilder;
        }

        public IRouter GetCurrent()
        {
            return _runningShellRouterTable.GetOrAdd(
                _shellSettings.Name,
                name => _routeBuilder.Build()
            );
        }

        public void Saved(ShellSettings settings)
        {
            _runningShellRouterTable.Remove(_shellSettings.Name);
        }
    }
}
