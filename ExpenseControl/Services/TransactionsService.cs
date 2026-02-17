using Microsoft.EntityFrameworkCore;
using ExpenseControl.Data;
using ExpenseControl.Models;

namespace ExpenseControl.Services
{
    public class TransactionsService
    {
        private readonly ApplicationDbContext _context;

        public TransactionsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .AsNoTracking()
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction != null)
            {
                {
                    _context.Transactions.Remove(transaction);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}