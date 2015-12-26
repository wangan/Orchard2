using Orchard.DependencyInjection;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeStepExecutor
    {
        bool ExecuteNextStep(string executionId);
    }
}