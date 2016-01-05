﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Recipes.Models;
using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeHarvester
    {
        /// <summary>
        /// Returns a collection of all recipes.
        /// </summary>
        Task<IEnumerable<Recipe>> HarvestRecipesAsync();

        /// <summary>
        /// Returns a collection of all recipes found in the specified extension.
        /// </summary>
        Task<IEnumerable<Recipe>> HarvestRecipesAsync(string extensionId);
    }

    public static class RecipeHarvesterExtensions
    {
        public static Recipe GetRecipeByName(this IEnumerable<Recipe> recipes, string recipeName)
        {
            return recipes.FirstOrDefault(r => r.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}