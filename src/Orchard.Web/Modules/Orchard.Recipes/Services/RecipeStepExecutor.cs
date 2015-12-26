using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Events;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Events;
using Orchard.Recipes.Records;
using System;
using System.Collections.Generic;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeStepExecutor : Component, IRecipeStepExecutor
    {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly ILogger _logger;

        public RecipeStepExecutor(
            IRecipeStepQueue recipeStepQueue,
            IEnumerable<IRecipeHandler> recipeHandlers,
            IEventBus eventBus,
            ISession session,
            ILogger<RecipeStepExecutor> logger)
        {

            _recipeStepQueue = recipeStepQueue;
            _recipeHandlers = recipeHandlers;
            _eventBus = eventBus;
            _session = session;
            _logger = logger;
        }

        public bool ExecuteNextStep(string executionId)
        {
            var nextRecipeStep = _recipeStepQueue.Dequeue(executionId);
            if (nextRecipeStep == null)
            {
                _logger.LogInformation("No more recipe steps left to execute.");
                _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(e => e.ExecutionComplete(executionId));
                return false;
            }

            _logger.LogInformation("Executing recipe step '{0}'.", nextRecipeStep.Name);

            var recipeContext = new RecipeContext { RecipeStep = nextRecipeStep, Executed = false, ExecutionId = executionId };

            try
            {
                _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(e => e.RecipeStepExecuting(executionId, recipeContext));

                foreach (var recipeHandler in _recipeHandlers)
                {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }

                UpdateStepResultRecord(executionId, nextRecipeStep.RecipeName, nextRecipeStep.Id, nextRecipeStep.Name, isSuccessful: true);
                _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(e => e.RecipeStepExecuted(executionId, recipeContext));
            }
            catch (Exception ex)
            {
                UpdateStepResultRecord(executionId, nextRecipeStep.RecipeName, nextRecipeStep.Id, nextRecipeStep.Name, isSuccessful: false, errorMessage: ex.Message);
                _logger.LogError(string.Format("Recipe execution failed because the step '{0}' failed.", nextRecipeStep.Name), ex);
                while (_recipeStepQueue.Dequeue(executionId) != null) ;
                var message = T("Recipe execution with ID {0} failed because the step '{1}' failed to execute. The following exception was thrown:\n{2}\nRefer to the error logs for more information.", executionId, nextRecipeStep.Name, ex.Message);
                throw new OrchardCoreException(message);
            }

            if (!recipeContext.Executed)
            {
                _logger.LogError("Recipe execution failed because no matching handler for recipe step '{0}' was found.", recipeContext.RecipeStep.Name);
                while (_recipeStepQueue.Dequeue(executionId) != null) ;
                var message = T("Recipe execution with ID {0} failed because no matching handler for recipe step '{1}' was found. Refer to the error logs for more information.", executionId, nextRecipeStep.Name);
                throw new OrchardCoreException(message);
            }

            return true;
        }

        private void UpdateStepResultRecord(string executionId, string recipeName, string stepId, string stepName, bool isSuccessful, string errorMessage = null)
        {
            var query = _session.QueryAsync<RecipeStepResult, RecipeStepResultIndex>().Where(
                record => record.ExecutionId == executionId && record.StepId == stepId && record.StepName == stepName);

            if (!string.IsNullOrWhiteSpace(recipeName))
                query = query.Where(record => record.RecipeName == recipeName);

            var stepResultRecord = query.FirstOrDefault().Result;

            if (stepResultRecord == null)
            {
                // No step result record was created when scheduling the step, so simply ignore.
                // The only reason where one would not create such a record would be Setup,
                // when no database exists to store the record but still wants to schedule a recipe step (such as the "StopViewsBackgroundCompilationStep").
                return;
            }

            stepResultRecord.IsCompleted = true;
            stepResultRecord.IsSuccessful = isSuccessful;
            stepResultRecord.ErrorMessage = errorMessage;

            _session.Save(stepResultRecord);
        }
    }
}