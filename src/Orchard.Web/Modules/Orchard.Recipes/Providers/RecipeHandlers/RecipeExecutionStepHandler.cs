using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Recipes.Providers.RecipeHandlers
{
    /// Delegates execution of the step to the appropriate recipe execution step implementation.
    /// </summary>
    public class RecipeExecutionStepHandler : Component, IRecipeHandler
    {
        private readonly IEnumerable<IRecipeExecutionStep> _recipeExecutionSteps;
        private readonly ILogger _logger;

        public RecipeExecutionStepHandler(IEnumerable<IRecipeExecutionStep> recipeExecutionSteps,
            ILogger<RecipeExecutionStepHandler> logger)
        {
            _recipeExecutionSteps = recipeExecutionSteps;
            _logger = logger;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext)
        {
            var executionStep = _recipeExecutionSteps.FirstOrDefault(x => x.Names.Contains(recipeContext.RecipeStep.Name));
            var recipeExecutionContext = new RecipeExecutionContext { ExecutionId = recipeContext.ExecutionId, RecipeStep = recipeContext.RecipeStep };

            if (executionStep != null)
            {
                _logger.LogInformation("Executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                executionStep.Execute(recipeExecutionContext);
                _logger.LogInformation("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                recipeContext.Executed = true;
            }
        }
    }
}
