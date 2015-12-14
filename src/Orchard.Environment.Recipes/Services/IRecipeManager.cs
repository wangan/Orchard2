using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeManager : IDependency
    {
        Task<string> ExecuteAsync(Recipe recipe);
    }
}