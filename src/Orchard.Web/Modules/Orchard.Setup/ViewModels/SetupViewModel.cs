using Orchard.Environment.Recipes.Models;
using Orchard.Setup.Annotations;
using System.Collections.Generic;

namespace Orchard.Setup.ViewModels
{
    public class SetupViewModel
    {
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }
        public string DatabaseProvider { get; set; }
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }

        public IEnumerable<Recipe> Recipes { get; set; }
        public string Recipe { get; set; }
        public string RecipeDescription { get; set; }
    }
}