﻿using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
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
            var sw = Stopwatch.StartNew();
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating shell context for tenant {0}", settings.Name);
            }

            var knownDescriptor = MinimumShellDescriptor();
            var blueprint = _compositionStrategy.Compose(settings, knownDescriptor);
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            var shellDescriptorManager = provider.GetService<IShellDescriptorManager>();
            var currentDescriptor = shellDescriptorManager.GetShellDescriptor().Result;

            if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Newer descriptor obtained. Rebuilding shell container.");
                }

                blueprint = _compositionStrategy.Compose(settings, currentDescriptor);
                (provider as IDisposable).Dispose();
                provider = _shellContainerFactory.CreateContainer(settings, blueprint);
            }

            try
            {
                var shellcontext = new ShellContext
                {
                    Settings = settings,
                    Blueprint = blueprint,
                    ServiceProvider = provider,
                };

                if (_logger.IsEnabled(LogLevel.Verbose))
                {
                    _logger.LogVerbose("Created shell context for tenant {0} in {1}ms", settings.Name, sw.ElapsedMilliseconds);
                }
                sw.Stop();
                return shellcontext;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError("Cannot create shell context", ex);
                throw;
            }
        }
        
        ShellContext IShellContextFactory.CreateSetupContext(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No shell settings available. Creating shell context for setup");
            }
            var descriptor = new ShellDescriptor
            {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "Orchard.Logging.Console" },
                    new ShellFeature { Name = "Orchard.Setup" },
                    new ShellFeature { Name = "Orchard.Recipes" }
                },
            };

            var blueprint = _compositionStrategy.Compose(settings, descriptor);
            var provider = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext
            {
                Settings = settings,
                Blueprint = blueprint,
                ServiceProvider = provider
            };
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