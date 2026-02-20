using ExpenseControl.Models;
using System.Collections;

namespace ExpenseControl.Services.Interfaces
{
    public interface ITransactionService : IBaseService<Transaction>
    {
        public Task<IEnumerable<TransactionItem>> GetTransactionItemsAsync(int transactionId);
    }
}
