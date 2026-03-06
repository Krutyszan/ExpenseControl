using ExpenseControl.Services.Interfaces;
namespace ExpenseControl.Services
{

    public enum TimePeriod { LastWeek, LastMonth, LastYear }
    public class DashboardService
    {
        private readonly ITransactionService _transactionService;

        public DashboardService(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<List<ChartDataPoint>> GetDataByAsync(TimePeriod period)
        {
            var allTransactions = await _transactionService.GetAllAsync();
            var startDate = period switch
            {
                TimePeriod.LastWeek => DateTime.Now.AddDays(-7),
                TimePeriod.LastMonth => DateTime.Now.AddMonths(-1),
                TimePeriod.LastYear => DateTime.Now.AddYears(-1),
            };

            var groupedData = allTransactions
                .Where(t => t.TransactionDate >= startDate)
                .SelectMany(t => t.Items)
                .GroupBy(i => i.Category.Id)
                .Select(g => new ChartDataPoint
                {
                    CategoryId = g.Key,
                    Value = (double)g.Sum(i => i.UnitPrice * i.Quantity),
                    Label = g.FirstOrDefault(i => i.Category != null)?.Category.Name
                })
                .OrderByDescending(dp => dp.Value)
                .ToList();

            return groupedData;
        }
    }
    public class ChartDataPoint
    {
        public int CategoryId { get; set; }
        public string Label { get; set; }
        public double Value { get; set; }
    }
}
