using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.Data.Migration;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Shell.Models;
using Orchard.Environment.Shell.State;
using Orchard.Hosting;
using Orchard.Hosting.ShellBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Setup.Services
{
    public class SetupService : Component, ISetupService
    {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IExtensionManager _extensionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IRunningShellRouterTable _runningShellRouterTable;
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IProcessingEngine _processingEngine;
        private readonly ILogger _logger;
        private IReadOnlyList<Recipe> _recipes;

        public SetupService(
            ShellSettings shellSettings,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            IShellContextFactory shellContextFactory,
            ICompositionStrategy compositionStrategy,
            IExtensionManager extensionManager,
            IHttpContextAccessor httpContextAccessor,
            IRunningShellTable runningShellTable,
            IRunningShellRouterTable runningShellRouterTable,
            IRecipeHarvester recipeHarvester,
            IProcessingEngine processingEngine,
            ILogger<SetupService> logger)
        {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _shellContextFactory = shellContextFactory;
            _compositionStrategy = compositionStrategy;
            _extensionManager = extensionManager;
            _httpContextAccessor = httpContextAccessor;
            _runningShellTable = runningShellTable;
            _runningShellRouterTable = runningShellRouterTable;
            _recipeHarvester = recipeHarvester;
            _processingEngine = processingEngine;
            _logger = logger;
        }

        public ShellSettings Prime()
        {
            return _shellSettings;
        }

        public IReadOnlyList<Recipe> Recipes()
        {
            if (_recipes == null)
            {
                var recipes = new List<Recipe>();
                recipes.AddRange(_recipeHarvester.HarvestRecipesAsync().Result.Where(recipe => recipe.IsSetupRecipe));
                _recipes = recipes;
            }
            return _recipes;
        }

        public string Setup(SetupContext context)
        {
            var initialState = _shellSettings.State;
            try
            {
                return SetupInternalAsync(context).Result;
            }
            catch
            {
                _shellSettings.State = initialState;
                throw;
            }
        }

        public async Task<string> SetupInternalAsync(SetupContext context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Running setup for tenant '{0}'.", _shellSettings.Name);
            }

            // Features to enable for Setup
            string[] hardcoded = {
                "Orchard.Logging.Console",
                "Orchard.Hosting",
                "Settings",
                "Orchard.Modules",
                "Orchard.Themes",
                "Orchard.Recipes"
                };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            // Set shell state to "Initializing" so that subsequent HTTP requests are responded to with "Service Unavailable" while Orchard is setting up.
            _shellSettings.State = TenantState.Initializing;

            var shellSettings = new ShellSettings(_shellSettings);

            if (string.IsNullOrEmpty(shellSettings.DatabaseProvider))
            {
                shellSettings.DatabaseProvider = context.DatabaseProvider;
                shellSettings.ConnectionString = context.DatabaseConnectionString;
                shellSettings.TablePrefix = context.DatabaseTablePrefix;
            }

            // TODO: Add Encryption Settings in

            var shellDescriptor = new ShellDescriptor
            {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name }).ToList()
            };

            var shellContext = _shellContextFactory.CreateDescribedContext(shellSettings, shellDescriptor);

            using (var environment = shellContext.CreateServiceScope())
            {
                var store = environment.ServiceProvider.GetService<IStore>();
                await store.InitializeAsync();

                // Give the chance for Setup default modules to run their migration
                await environment
                    .ServiceProvider
                    .GetService<IDataMigrationManager>()
                    .UpdateAsync(context.EnabledFeatures);

                // Create the shell descriptor
                await environment
                    .ServiceProvider
                    .GetService<IShellDescriptorManager>()
                    .UpdateShellDescriptorAsync(
                        0,
                        shellDescriptor.Features,
                        shellDescriptor.Parameters);
            }

            // In effect "pump messages" see PostMessage circa 1980.
            while (_processingEngine.AreTasksPending())
            {
                _processingEngine.ExecuteNextTask();
            }

            string executionId;
            using (var environment = _orchardHost.CreateShellContext(shellSettings))
            {
                executionId = CreateTenantData(context, environment);
            }

            shellSettings.State = TenantState.Running;
            _runningShellRouterTable.Remove(shellSettings.Name);
            _orchardHost.UpdateShellSettings(shellSettings);
            return executionId;
        }

        private string CreateTenantData(SetupContext context, ShellContext shellContext)
        {
            var recipeManager = shellContext.ServiceProvider.GetService<IRecipeManager>();
            var recipe = context.Recipe;
            var executionId = recipeManager.ExecuteAsync(recipe).Result;

            // Once the recipe has finished executing, we need to update the shell state to "Running", so add a recipe step that does exactly that.
            JObject activateShellJSteps = new JObject();
            JObject activateShellJStep = new JObject();
            activateShellJStep.Add("name", "ActivateShell");
            activateShellJSteps.Add("steps", activateShellJStep);

            var activateShellStep = new RecipeStep(
                Guid.NewGuid().ToString("N"), 
                recipe.Name, 
                "ActivateShell",
                activateShellJSteps);

            recipeManager.ExecuteRecipeStep(executionId, activateShellStep);

            return executionId;
        }
    }
}