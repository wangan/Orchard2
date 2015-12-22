using Microsoft.AspNet.Razor.TagHelpers;
using Orchard.Environment.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Setup.TagHelpers
{
    [HtmlTargetElement("orchard-recipe", Attributes = "recipe")]
    public class RecipeTagHelper : TagHelper
    {
        public Recipe Recipe { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("data-recipe-description", Recipe.Description);
            output.Attributes.Add("value", Recipe.Name);
            output.Attributes.Add("text", Recipe.Name);

            return base.ProcessAsync(context, output);
        }
    }
}
