using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeExecutor : IDependency
    {
        Task<string> ExecuteAsync(Recipe recipe);
    }
}