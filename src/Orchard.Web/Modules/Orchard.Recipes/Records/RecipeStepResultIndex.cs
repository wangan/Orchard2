using Orchard.DependencyInjection;
using Orchard.Recipes.Models;
using YesSql.Core.Indexes;

namespace Orchard.Recipes.Records
{
    public class RecipeStepResultIndex : MapIndex
    {
        public string ExecutionId { get; set; }
        public string StepId { get; set; }
        public string StepName { get; set; }
        public string RecipeName { get; set; }
    }

    public class RecipeStepResultIndexProvider : IndexProvider<RecipeStepResultRecord>, IDependency
    {
        public override void Describe(DescribeContext<RecipeStepResultRecord> context)
        {
            context.For<RecipeStepResultIndex>()
                .Map(contentItem => new RecipeStepResultIndex
                {
                    ExecutionId = contentItem.ExecutionId,
                    StepId = contentItem.StepId,
                    StepName = contentItem.StepName,
                    RecipeName = contentItem.RecipeName
                });
        }
    }
}