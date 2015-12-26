namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeScheduler
    {
        void ScheduleWork(string executionId);
    }
}