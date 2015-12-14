using Newtonsoft.Json.Linq;

namespace Orchard.Environment.Recipes.Services
{
    public class UpdateRecipeExecutionStepContext
    {
        public JObject RecipeDocument { get; set; }
        public JObject Step { get; set; }
    }
}