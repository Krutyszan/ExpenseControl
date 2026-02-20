namespace ExpenseControl.Services.Interfaces
{
    public interface IAIService
    {
        Task<String> AnalyzeImageAsync(Stream imageStream, string contentType, string prompt);
        Task<String> GenerateTextAsync(string prompt);

    }
}
