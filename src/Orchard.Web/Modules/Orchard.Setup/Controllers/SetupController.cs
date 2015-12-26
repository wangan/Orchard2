using Microsoft.AspNet.Mvc;
using Orchard.Localization;
using Orchard.Environment.Shell;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using System;
using System.Linq;
using Orchard.Environment.Recipes.Services;

namespace Orchard.Setup.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";

        public SetupController(ISetupService setupService,
            ShellSettings shellSettings)
        {
            _setupService = setupService;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            var initialSettings = _setupService.Prime();
            var recipes = _setupService.Recipes();
            string recipeDescription = null;

            if (recipes.Any())
            {
                recipeDescription = recipes[0].Description;
            }

            return IndexViewResult(new SetupViewModel
            {
                Recipes = recipes,
                RecipeDescription = recipeDescription
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model)
        {
            var recipes = _setupService.Recipes().ToList();

            if (model.Recipe == null)
            {
                if (!(recipes.Select(r => r.Name).Contains(DefaultRecipe)))
                {
                    ModelState.AddModelError("Recipe", T("No recipes were found."));
                }
                else {
                    model.Recipe = DefaultRecipe;
                }
            }
            if (!ModelState.IsValid)
            {
                model.Recipes = recipes;
                foreach (var rec in recipes.Where(r => r.Name == model.Recipe))
                {
                    model.RecipeDescription = rec.Description;
                }

                return IndexViewResult(model);
            }


            var recipe = recipes.GetRecipeByName(model.Recipe);
            var setupContext = new SetupContext
            {
                SiteName = model.SiteName,
                DatabaseProvider = model.DatabaseProvider,
                DatabaseConnectionString = model.ConnectionString,
                DatabaseTablePrefix = model.TablePrefix,
                EnabledFeatures = null, // default list
                Recipe = recipe
            };

            var executionId = _setupService.Setup(setupContext);

            var urlPrefix = "";
            if (!string.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix))
            {
                urlPrefix = _shellSettings.RequestUrlPrefix + "/";
            }

            // Redirect to the welcome page.
            // TODO: Redirect on the home page once we don't rely on Orchard.Demo
            return Redirect("~/" + urlPrefix + "home/index");
        }
    }
}