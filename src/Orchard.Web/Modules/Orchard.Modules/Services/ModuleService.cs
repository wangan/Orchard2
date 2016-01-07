using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Shell.Descriptor;
using Orchard.FileSystem.VirtualPath;
using Orchard.Localization;
using Orchard.Modules.Models;
using Orchard.Notifications;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Modules.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IFeatureManager _featureManager;
        private readonly INotifier _notifier;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IClock _clock;

        public ModuleService(
                IFeatureManager featureManager,
                INotifier notifier,
                IVirtualPathProvider virtualPathProvider,
                IExtensionManager extensionManager,
                IShellDescriptorManager shellDescriptorManager,
                IClock clock)
        {
            _featureManager = featureManager;
            _notifier = notifier;
            _virtualPathProvider = virtualPathProvider;
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
            _clock = clock;

            if (_featureManager.FeatureDependencyNotification == null)
            {
                _featureManager.FeatureDependencyNotification = GenerateWarning;
            }

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Retrieves an enumeration of the available features together with its state (enabled / disabled).
        /// </summary>
        /// <returns>An enumeration of the available features together with its state (enabled / disabled).</returns>
        public async Task<IEnumerable<ModuleFeature>> GetAvailableFeatures()
        {
            var enabledFeatures = (await _shellDescriptorManager.GetShellDescriptorAsync()).Features;
            return _extensionManager.AvailableExtensions()
                .SelectMany(m => _extensionManager.LoadFeatures(m.Features))
                .Select(f => AssembleModuleFromDescriptor(f, enabledFeatures
                    .FirstOrDefault(sf => string.Equals(sf.Name, f.Descriptor.Id, StringComparison.OrdinalIgnoreCase)) != null));
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        public async Task EnableFeatures(IEnumerable<string> featureIds)
        {
            await EnableFeatures(featureIds, false);
        }

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        public async Task EnableFeatures(IEnumerable<string> featureIds, bool force)
        {
            var features = await _featureManager.GetAvailableFeatures();
            foreach (string featureId in await _featureManager.EnableFeatures(featureIds, force))
            {
                var featureName = features
                    .Single(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                _notifier.Information(T("{0} was enabled", featureName));
            }
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        public async Task DisableFeatures(IEnumerable<string> featureIds)
        {
            await DisableFeatures(featureIds, false);
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        public async Task DisableFeatures(IEnumerable<string> featureIds, bool force)
        {
            var features = await _featureManager.GetAvailableFeatures();
            foreach (string featureId in await _featureManager.DisableFeatures(featureIds, force))
            {
                var featureName = features
                    .Single(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                _notifier.Information(T("{0} was disabled", featureName));
            }
        }

        /// <summary>
        /// Determines if a module was recently installed by using the project's last written time.
        /// </summary>
        /// <param name="extensionDescriptor">The extension descriptor.</param>
        public bool IsRecentlyInstalled(ExtensionDescriptor extensionDescriptor)
        {
            DateTimeOffset lastWrittenUtc = _clock.UtcNow;
            string projectFile = GetManifestPath(extensionDescriptor);
            if (!string.IsNullOrEmpty(projectFile))
            {
                // If project file was modified less than 24 hours ago, the module was recently deployed
                lastWrittenUtc = _virtualPathProvider.GetFileLastWriteTimeUtc(projectFile);
            }

            return _clock.UtcNow.Subtract(lastWrittenUtc) < new TimeSpan(1, 0, 0, 0);
        }

        public async Task<IEnumerable<FeatureDescriptor>> GetDependentFeatures(string featureId)
        {
            var dependants = await _featureManager.GetDependentFeatures(featureId);
            var features = await _featureManager.GetAvailableFeatures();
            
            var availableFeatures = features.ToLookup(f => f.Id, StringComparer.OrdinalIgnoreCase);

            return dependants
                .SelectMany(id => availableFeatures[id])
                .ToList();
        }

        /// <summary>
        /// Retrieves the full path of the manifest file for a module's extension descriptor.
        /// </summary>
        /// <param name="extensionDescriptor">The module's extension descriptor.</param>
        /// <returns>The full path to the module's manifest file.</returns>
        private string GetManifestPath(ExtensionDescriptor extensionDescriptor)
        {
            string projectPath = _virtualPathProvider.Combine(extensionDescriptor.Location, extensionDescriptor.Id, "module.txt");

            if (!_virtualPathProvider.FileExists(projectPath))
            {
                return null;
            }

            return projectPath;
        }

        private static ModuleFeature AssembleModuleFromDescriptor(Feature feature, bool isEnabled)
        {
            return new ModuleFeature
            {
                Descriptor = feature.Descriptor,
                IsEnabled = isEnabled
            };
        }

        private void GenerateWarning(string messageFormat, string featureName, IEnumerable<string> featuresInQuestion)
        {
            if (!featuresInQuestion.Any())
                return;

            _notifier.Warning(T(
                messageFormat,
                featureName,
                featuresInQuestion.Count() > 1
                    ? string.Join("",
                                  featuresInQuestion.Select(
                                      (fn, i) =>
                                      T(i == featuresInQuestion.Count() - 1
                                            ? "{0}"
                                            : (i == featuresInQuestion.Count() - 2
                                                   ? "{0} and "
                                                   : "{0}, "), fn).ToString()).ToArray())
                    : featuresInQuestion.First()));
        }
    }
}