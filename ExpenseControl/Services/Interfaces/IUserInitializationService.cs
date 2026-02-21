namespace ExpenseControl.Services.Interfaces
{
    public interface IUserInitializationService
    {
        Task SeedDefaultUserDataAsync(string UserId);
    }
}
