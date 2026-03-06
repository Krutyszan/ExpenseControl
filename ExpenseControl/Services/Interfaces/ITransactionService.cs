using ExpenseControl.Models;

namespace ExpenseControl.Services.Interfaces
{
    public interface ITransactionService : IBaseService<Transaction>
    {
        public Task<IEnumerable<TransactionItem>> GetTransactionItemsAsync(int transactionId);
    }
}
