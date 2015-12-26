using Newtonsoft.Json.Linq;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeParser
    {
        Recipe ParseRecipe(JObject recipeDocument);
    }

    public static class RecipeParserExtensions
    {
        public static Recipe ParseRecipe(this IRecipeParser recipeParser, string recipeText)
        {
            var recipeDocument = JObject.Parse(recipeText);
            return recipeParser.ParseRecipe(recipeDocument);
        }
    }
}