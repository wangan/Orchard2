using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Hosting;
using Orchard.Hosting.Web.Routing.Routes;
using Orchard.Recipes;

namespace Orchard.Setup
{
    public class SetupMode : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            new ShellModule().Configure(serviceCollection);
            new RecipeModule().Configure(serviceCollection);

            serviceCollection.AddScoped<IRoutePublisher, RoutePublisher>();
        }
    }
}