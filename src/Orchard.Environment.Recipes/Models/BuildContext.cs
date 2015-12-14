using Newtonsoft.Json.Linq;

namespace Orchard.Environment.Recipes.Services
{
    public class BuildContext
    {
        public JObject RecipeDocument { get; set; }
    }
}