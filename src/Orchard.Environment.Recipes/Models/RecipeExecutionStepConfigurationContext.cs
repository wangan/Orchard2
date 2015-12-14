using Newtonsoft.Json.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class RecipeExecutionStepConfigurationContext : ConfigurationContext
    {
        public RecipeExecutionStepConfigurationContext(JObject configurationElement) : base(configurationElement)
        {
        }
    }
}