using Microsoft.AspNet.Routing;

namespace Orchard.Hosting.Routing
{
    public interface IRunningShellRouterManager
    {
        IRouter GetCurrent();
    }
}
