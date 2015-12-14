using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Environment.Recipes.Services
{
    /// <summary>
    /// Provides information about the result of recipe execution.
    /// </summary>
    public interface IRecipeResultAccessor : IDependency
    {
        Task<RecipeResult> GetResultAsync(string executionId);
    }
}