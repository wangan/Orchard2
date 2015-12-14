using Newtonsoft.Json.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class ConfigurationContext
    {
        protected ConfigurationContext(JObject configurationElement)
        {
            ConfigurationElement = configurationElement;
        }

        public JObject ConfigurationElement { get; set; }
    }
}