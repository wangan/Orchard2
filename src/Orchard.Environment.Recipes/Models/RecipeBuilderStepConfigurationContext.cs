using Newtonsoft.Json.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class RecipeBuilderStepConfigurationContext : ConfigurationContext
    {
        public RecipeBuilderStepConfigurationContext(JObject configurationElement) : base(configurationElement)
        {
        }
    }
}