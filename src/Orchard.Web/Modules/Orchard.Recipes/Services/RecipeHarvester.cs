using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Environment.Recipes.Models;
using Microsoft.Extensions.Logging;
using Orchard.FileSystem;
using Orchard.Environment.Recipes.Services;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeHarvester : IRecipeHarvester
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IClientFolder _webSiteFolder;
        private readonly IRecipeParser _recipeParser;
        private readonly ILogger _logger;

        public RecipeHarvester(
            IExtensionManager extensionManager,
            IClientFolder webSiteFolder,
            IRecipeParser recipeParser,
            ILogger<RecipeHarvester> logger)
        {
            _extensionManager = extensionManager;
            _webSiteFolder = webSiteFolder;
            _recipeParser = recipeParser;
            _logger = logger;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public async Task<IEnumerable<Recipe>> HarvestRecipesAsync()
        {
            var extensions = await Task.FromResult(_extensionManager.AvailableExtensions());
            return extensions.SelectMany(HarvestRecipes);
        }

        public async Task<IEnumerable<Recipe>> HarvestRecipesAsync(string extensionId)
        {
            var extension = _extensionManager.GetExtension(extensionId);
            if (extension != null)
            {
                return await Task.FromResult(HarvestRecipes(extension));
            }

            _logger.LogError("Could not discover recipes because module '{0}' was not found.", extensionId);
            return Enumerable.Empty<Recipe>();
        }

        private IEnumerable<Recipe> HarvestRecipes(ExtensionDescriptor extension)
        {
            var recipes = new List<Recipe>();

            var recipeLocation = Path.Combine(extension.Location, extension.Id, "Recipes");
            var recipeFiles = _webSiteFolder.ListFiles(recipeLocation, true);

            recipeFiles.Where(r => r.EndsWith(".recipe.json", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(r => {
                try
                {
                    recipes.Add(_recipeParser.ParseRecipe(_webSiteFolder.ReadFile(r)));
                }
                catch (Exception ex)
                {
                    _logger.LogError(string.Format("Error while parsing recipe file '{0}'.", r), ex);
                }
            });

            return recipes;
        }
    }
}