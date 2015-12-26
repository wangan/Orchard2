using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Services;
using Orchard.Recipes.Services;

namespace Orchard.Recipes
{
    public class RecipeModule : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IRecipeHarvester, RecipeHarvester>();
            serviceCollection.AddScoped<IRecipeManager, RecipeManager>();
            serviceCollection.AddScoped<IRecipeParser, RecipeParser>();
            serviceCollection.AddScoped<IRecipeStepExecutor, RecipeStepExecutor>();
            serviceCollection.AddSingleton<IRecipeStepQueue, RecipeStepQueue>();
            serviceCollection.AddScoped<IRecipeScheduler, RecipeScheduler>();
        }
    }
}
