using Newtonsoft.Json.Linq;
using Orchard.DependencyInjection;
using Orchard.Services;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Recipes.Services
{
    public class RecipeBuilder : Component, IRecipeBuilder
    {
        private readonly IClock _clock;

        public RecipeBuilder(IClock clock)
        {
            _clock = clock;
        }

        public JObject Build(IEnumerable<IRecipeBuilderStep> steps)
        {
            var context = new BuildContext
            {
                RecipeDocument = CreateRecipeRoot()
            };

            foreach (var step in steps.OrderByDescending(x => x.Priority))
            {
                step.Build(context);
            }

            return context.RecipeDocument;
        }

        private JObject CreateRecipeRoot()
        {
            dynamic recipeRoot = new JObject();
            recipeRoot._comment = T("Exported from Orchard").ToString();
            recipeRoot.ExportUtc = _clock.UtcNow;
            return recipeRoot;
        }
    }
}