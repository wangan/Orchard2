using System;
using System.Linq;
using Orchard.Environment.Extensions.Features;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Recipes.Services;
using Orchard.Environment.Recipes.Models;
using Newtonsoft.Json.Linq;

namespace Orchard.Modules.Recipes.Executors
{
    public class FeatureStep : RecipeExecutionStep
    {
        private readonly IFeatureManager _featureManager;

        public FeatureStep(
            IFeatureManager featureManager,
            ILogger<FeatureStep> logger) : base(logger)
        {

            _featureManager = featureManager;
        }

        public override string Name
        {
            get { return "Feature"; }
        }

        // <Feature enable="f1,f2,f3" disable="f4" />
        // Enable/Disable features.
        public override void Execute(RecipeExecutionContext recipeContext)
        {
            var featuresToDisable = recipeContext
                .RecipeStep
                .Step["disable"]
                .Children()
                .Select(x => x.Value<string>());
            
            var featuresToEnable = recipeContext
                .RecipeStep
                .Step["enable"]
                .Children()
                .Select(x => x.Value<string>());
            
            var availableFeatures = _featureManager.GetAvailableFeatures().Result.Select(x => x.Id).ToArray();
            foreach (var featureName in featuresToDisable)
            {
                if (!availableFeatures.Contains(featureName))
                {
                    throw new InvalidOperationException(string.Format("Could not disable feature {0} because it was not found.", featureName));
                }
            }

            foreach (var featureName in featuresToEnable)
            {
                if (!availableFeatures.Contains(featureName))
                {
                    throw new InvalidOperationException(string.Format("Could not enable feature {0} because it was not found.", featureName));
                }
            }

            if (featuresToDisable.Any())
            {
                Logger.LogInformation("Disabling features: {0}", string.Join(";", featuresToDisable));
                _featureManager.DisableFeatures(featuresToDisable, true);
            }
            if (featuresToEnable.Any())
            {
                Logger.LogInformation("Enabling features: {0}", string.Join(";", featuresToEnable));
                _featureManager.EnableFeatures(featuresToEnable, true);
            }
        }
    }
}