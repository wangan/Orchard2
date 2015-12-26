using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Orchard.Recipes.Services
{
    public class RecipeParser : Component, IRecipeParser
    {
        private readonly ILogger _logger;

        public RecipeParser(ILogger<RecipeParser> logger) : base() {
            _logger = logger;
        }

        public Recipe ParseRecipe(JObject recipeDocument)
        {
            var recipe = new Recipe();
            
            recipe.Name = recipeDocument.Value<string>("name");
            recipe.Description = recipeDocument.Value<string>("description");
            recipe.Author = recipeDocument.Value<string>("author");
            recipe.WebSite = recipeDocument.Value<string>("website");
            recipe.Version = recipeDocument.Value<string>("version");
            recipe.IsSetupRecipe = recipeDocument.Value<bool>("issetuprecipe");
            recipe.ExportUtc = recipeDocument.Value<DateTime?>("exportutc");
            recipe.Category = recipeDocument.Value<string>("category");
            recipe.Tags = recipeDocument.Value<string>("tags");

            var recipeSteps = new List<RecipeStep>();

            var documentSteps = recipeDocument["steps"];
            var stepId = 0;
            foreach (var rs in documentSteps.Children()) {
                var recipeStep = new RecipeStep(
                    id: (++stepId).ToString(CultureInfo.InvariantCulture),
                    recipeName: recipe.Name,
                    name: rs.Value<string>("name"),
                    step: rs);
                recipeSteps.Add(recipeStep);
            }

            recipe.RecipeSteps = recipeSteps;

            return recipe;
        }
    }
}