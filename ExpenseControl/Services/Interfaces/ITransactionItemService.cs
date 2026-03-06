using ExpenseControl.Models;

namespace ExpenseControl.Services.Interfaces
{
    public interface ITransactionItemService : IBaseService<TransactionItem>
    {
        Task<IEnumerable<TransactionItem>> GetByTransactionIdAsync(int transactionId);
    }
}
