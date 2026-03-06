using ExpenseControl.Data;
using ExpenseControl.Models;
using ExpenseControl.Services.Base;
using ExpenseControl.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Services
{
    public class TransactionItemService : BaseService<TransactionItem>, ITransactionItemService
    {
        public TransactionItemService(ApplicationDbContext context) : base(context)
        { }
        public override async Task<IEnumerable<TransactionItem>> GetAllAsync()
        {
            return await _dbSet
                .Include(i => i.Category) // Ważne: dociągamy kategorię, żeby widzieć ją w tabeli
                .OrderByDescending(item => item.Quantity * item.UnitPrice)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task UpdateAsync(TransactionItem item)
        {
            // 1. Zabezpieczenie: "Zapomnij" o obiekcie kategorii.
            // Dzięki temu EF nie będzie próbował go śledzić ani aktualizować.
            // Zaktualizuje się tylko relacja po kluczu obcym (CategoryId).
            item.Category = null;
            // 2. Jeśli masz tam też Store, zrób to samo, na wszelki wypadek
            // item.Store = null; (jeśli TransactionItem ma bezpośrednie łącze do Store)

            // 3. Teraz bezpieczny update
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<TransactionItem>> GetByTransactionIdAsync(int transactionId)
        {
            return await _dbSet
                .Where(i => i.TransactionId == transactionId)
                .Include(i => i.Category)
                .OrderByDescending(item => item.Quantity * item.UnitPrice)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}