using Orchard.Environment.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Environment.Recipes.Models;
using Microsoft.Extensions.Logging;
using Orchard.FileSystem.AppData;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Orchard.Recipes.Services
{
    public class RecipeStepQueue : IRecipeStepQueue
    {
        private readonly ILogger _logger;
        private readonly IAppDataFolder _appDataFolder;

        private readonly string _recipeQueueFolder = "RecipeQueue";

        public RecipeStepQueue(ILogger<RecipeStepQueue> logger,
            IAppDataFolder appDataFolder) {
            _logger = logger;
            _appDataFolder = appDataFolder;
        }

        public void Enqueue(string executionId, RecipeStep step)
        {
            _logger.LogInformation("Enqueuing recipe step '{0}'.", step.Name);
            var recipeStepElement = new JObject();
            recipeStepElement.Add(new JProperty("id", step.Id));
            recipeStepElement.Add(new JProperty("recipename", step.RecipeName));
            recipeStepElement.Add(new JProperty("name", step.Name));
            recipeStepElement.Add(new JProperty("step", step.Step));

            if (_appDataFolder.DirectoryExists(_appDataFolder.Combine(_recipeQueueFolder, executionId)))
            {
                int stepIndex = GetLastStepIndex(executionId) + 1;
                _appDataFolder.CreateFile(_appDataFolder.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + stepIndex),
                                          recipeStepElement.ToString());
            }
            else {
                _appDataFolder.CreateFile(
                    _appDataFolder.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + "0"),
                    recipeStepElement.ToString());
            }
        }

        public RecipeStep Dequeue(string executionId)
        {
            _logger.LogInformation("Dequeuing recipe steps.");
            if (!_appDataFolder.DirectoryExists(_appDataFolder.Combine(_recipeQueueFolder, executionId)))
            {
                return null;
            }

            RecipeStep recipeStep = null;
            int stepIndex = GetFirstStepIndex(executionId);

            if (stepIndex >= 0)
            {
                var stepPath = Path.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + stepIndex);
                // string to xelement
                var stepElement = JObject.Parse(_appDataFolder.ReadFile(stepPath));
                var stepName = stepElement.Value<string>("name");
                var recipeName = stepElement.Value<string>("recipename");
                var stepId = stepElement.Value<string>("id");
                
                _logger.LogInformation("Dequeuing recipe step '{0}'.", stepName);
                recipeStep = new RecipeStep(id: stepId, recipeName: recipeName, name: stepName, step: stepElement["step"]);
                _appDataFolder.DeleteFile(stepPath);
            }

            if (stepIndex < 0)
            {
                _appDataFolder.DeleteFile(_appDataFolder.Combine(_recipeQueueFolder, executionId));
            }

            return recipeStep;
        }

        private int GetFirstStepIndex(string executionId)
        {
            var stepFiles = _appDataFolder.ListFiles(_appDataFolder.Combine(_recipeQueueFolder, executionId));
            if (!stepFiles.Any())
                return -1;
            var currentSteps = stepFiles.Select(stepFile => int.Parse(stepFile.Name.Substring(stepFile.Name.LastIndexOf('/') + 1))).ToList();
            currentSteps.Sort();
            return currentSteps[0];
        }

        private int GetLastStepIndex(string executionId)
        {
            int lastIndex = -1;
            var stepFiles = _appDataFolder.ListFiles(_appDataFolder.Combine(_recipeQueueFolder, executionId));
            // we always have only a handful of steps.
            foreach (var stepFile in stepFiles)
            {
                int stepOrder = int.Parse(stepFile.Name.Substring(stepFile.Name.LastIndexOf('/') + 1));
                if (stepOrder > lastIndex)
                    lastIndex = stepOrder;
            }

            return lastIndex;
        }
    }
}
