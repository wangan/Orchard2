using Microsoft.Extensions.Logging;
using Orchard.Environment.Recipes.Events;
using Orchard.Environment.Recipes.Services;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.State;
using Orchard.Events;
using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public class RecipeScheduler : IRecipeScheduler, IRecipeSchedulerEventHandler
    {
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IRecipeStepExecutor _recipeStepExecutor;
        private readonly IEventBus _events;

        private readonly ILogger _logger;

        private readonly ContextState<string> _executionIds = new ContextState<string>("executionid");

        public RecipeScheduler(
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            IRecipeStepExecutor recipeStepExecutor,
            IEventBus events,
            ILogger<RecipeScheduler> logger) {
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _recipeStepExecutor = recipeStepExecutor;
            _events = events;
            _logger = logger;
        }

        public void ExecuteWork(string executionId)
        {
            _executionIds.SetState(executionId);
            try
            {
                // todo: this callback should be guarded against concurrency by the IProcessingEngine
                var scheduleMore = _recipeStepExecutor.ExecuteNextStep(executionId);
                if (scheduleMore)
                {
                    _logger.LogInformation("Scheduling next step of recipe.");
                    ScheduleWork(executionId);
                }
                else {
                    _logger.LogInformation("All recipe steps executed; restarting shell.");

                    // Because recipes execute in their own workcontext, we need to restart the shell, as signaling a cache won't work across workcontexts.
                    _events.NotifyAsync<IShellDescriptorManagerEventHandler>(e => e.Changed(
                        _shellDescriptorManager.GetShellDescriptorAsync().Result, 
                        _shellSettings.Name));
                }
            }
            finally
            {
                _executionIds.SetState(null);
            }
        }

        public void ScheduleWork(string executionId)
        {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptorAsync().Result;

            _processingEngine.AddTask(
                _shellSettings,
                shellDescriptor,
                "IRecipeSchedulerEventHandler.ExecuteWork",
                new Dictionary<string, object> { { "executionId", executionId } });
        }
    }
}
