using Newtonsoft.Json.Linq;
using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeBuilder : IDependency
    {
        JObject Build(IEnumerable<IRecipeBuilderStep> steps);
    }
}