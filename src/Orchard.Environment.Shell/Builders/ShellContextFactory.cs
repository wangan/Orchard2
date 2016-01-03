using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Hosting.ShellBuilders;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;
using Orchard.Environment.Shell.Descriptor;

namespace Orchard.Environment.Shell.Builders
{
    public class ShellContextFactory : IShellContextFactory
    {
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ILogger _logger;

        public ShellContextFactory(
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory,
            ILogger<ShellContextFactory> logger)
        {
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            _logger = logger;
        }

        ShellContext IShellContextFactory.CreateShellContext(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating shell context for tenant {0}", settings.Name);
            }

            var knownDescriptor = MinimumShellDescriptor();
            var initialContext = CreateDescribedContext(settings, knownDescriptor);
            var shellDescriptorManager = initialContext.ServiceProvider.GetService<IShellDescriptorManager>();
            ShellDescriptor currentDescriptor = shellDescriptorManager.GetShellDescriptor().Result;
            
            if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Newer descriptor obtained. Rebuilding shell container.");
                }

                initialContext.Dispose();
                return CreateDescribedContext(settings, currentDescriptor);
            }

            return initialContext;
        }

        public ShellContext CreateDescribedContext(ShellSettings settings, ShellDescriptor shellDescriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating described context for tenant {0}", settings.Name);
            }

            var blueprint = _compositionStrategy.Compose(settings, shellDescriptor);
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext
            {
                Settings = settings,
                Blueprint = blueprint,
                ServiceProvider = provider
            };
        }

        private static ShellDescriptor MinimumShellDescriptor()
        {
            return new ShellDescriptor
            {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "Orchard.Logging.Console" },
                    new ShellFeature { Name = "Orchard.Hosting" },
                    new ShellFeature { Name = "Settings" }
                },
                Parameters = new List<ShellParameter>()
            };
        }
    }
}