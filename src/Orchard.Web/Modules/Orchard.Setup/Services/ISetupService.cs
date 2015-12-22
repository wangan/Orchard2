using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Shell;
using System.Collections.Generic;

namespace Orchard.Setup.Services
{
    public interface ISetupService : IDependency
    {
        ShellSettings Prime();
        IReadOnlyList<Recipe> Recipes();
        string Setup(SetupContext context);
    }
}