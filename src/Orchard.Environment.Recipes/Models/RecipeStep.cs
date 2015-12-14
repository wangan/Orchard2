using Newtonsoft.Json.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class RecipeStep
    {
        public RecipeStep(string id, string recipeName, string name, JObject step)
        {
            Id = id;
            RecipeName = recipeName;
            Name = name;
            Step = step;
        }

        public string Id { get; set; }
        public string RecipeName { get; private set; }
        public string Name { get; private set; }
        public JObject Step { get; private set; }
    }
}