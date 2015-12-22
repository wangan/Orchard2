namespace Orchard.Recipes.Records
{
    public class RecipeStepResultRecord
    {
        public string ExecutionId { get; set; }
        public string RecipeName { get; set; }
        public string StepId { get; set; }
        public string StepName { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}