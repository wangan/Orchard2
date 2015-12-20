using Orchard.Recipes.Providers.RecipeHandlers;
using Xunit;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers
{
    public class RecipeExecutionStepHandlerTest
    {
        private IServiceProvider _services;

        public RecipeExecutionStepHandlerTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ILogger<StubRecipeExecutionStep>, NullLogger<StubRecipeExecutionStep>>();
            serviceCollection.AddScoped<ILogger<RecipeExecutionStepHandler>, NullLogger<RecipeExecutionStepHandler>>();
            serviceCollection.AddSingleton<IRecipeExecutionStep, StubRecipeExecutionStep>();
            serviceCollection.AddSingleton<RecipeExecutionStepHandler>();

            _services = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void ExecuteRecipeExecutionStepHandlerTest()
        {
            var handlerUnderTest = _services.GetService<RecipeExecutionStepHandler>();
            var fakeRecipeStep = (StubRecipeExecutionStep)_services.GetService<IRecipeExecutionStep>();
            
            var context = new RecipeContext
            {
                RecipeStep = new RecipeStep(
                    id: "1", 
                    recipeName: "FakeRecipe", 
                    name: "FakeRecipeStep", 
                    step: new JObject()),
                ExecutionId = "12345"
            };

            handlerUnderTest.ExecuteRecipeStep(context);

            Assert.True(fakeRecipeStep.IsExecuted);
            Assert.True(context.Executed);
        }
    }

    public class StubRecipeExecutionStep : RecipeExecutionStep
    {

        public StubRecipeExecutionStep(
            ILogger<StubRecipeExecutionStep> logger) : base(logger)
        {
        }

        public override string Name
        {
            get { return "FakeRecipeStep"; }
        }

        public bool IsExecuted { get; set; }

        public override void Execute(RecipeExecutionContext context)
        {
            IsExecuted = true;
        }
    }
}
