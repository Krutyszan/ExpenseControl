using Microsoft.EntityFrameworkCore;
using ExpenseControl.Data;
using ExpenseControl.Models;
using ExpenseControl.Services.Base;
using ExpenseControl.Services.Interfaces;

namespace ExpenseControl.Services
{
    public class TransactionService : BaseService<Transaction>, ITransactionService
    {
        public TransactionService(ApplicationDbContext context) : base(context)
        {
        }
        public override async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.Store)
                    .ThenInclude(s => s.Category)
                .Include(t => t.Items)
                .OrderByDescending(t => t.TransactionDate)
                .AsNoTracking()
                .ToListAsync();
        }
        public override async Task AddAsync(Transaction transaction)
        {
            if (transaction.Store != null && !string.IsNullOrWhiteSpace(transaction.Store.Name))
            {
                var cleanName = transaction.Store.Name.Trim();
                transaction.Store.Name = cleanName;

                var existingStore = await _context.Stores
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == cleanName.ToLower());

                if (existingStore != null)
                {
                    transaction.StoreId = existingStore.Id;

                    // LUKA 1 ZAMKNIĘTA: Paragon dostaje kategorię istniejącego sklepu
                    transaction.CategoryId = existingStore.CategoryId;

                    transaction.Store = null;
                }
                else
                {
                    // LUKA 2 ZAMKNIĘTA: Resetujemy wymyślone przez AI ID sklepu do zera.
                    // Baza SQLite wygeneruje teraz poprawne, unikalne ID.
                    transaction.Store.Id = 0;

                    if (transaction.Store.CategoryId == 0)
                    {
                        var defaultCat = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Inne");
                        transaction.Store.CategoryId = defaultCat?.Id ?? 1;
                    }

                    // LUKA 3 ZAMKNIĘTA: Paragon dziedziczy kategorię po nowo utworzonym sklepie
                    transaction.CategoryId = transaction.Store.CategoryId;
                }
            }
            else if (transaction.StoreId == 0)
            {
                var unknownStore = await GetOrCreateStoreAsync("Nieznany Sklep");
                transaction.StoreId = unknownStore.Id;

                // LUKA 4 ZAMKNIĘTA: Paragon dostaje kategorię Nieznanego Sklepu
                transaction.CategoryId = unknownStore.CategoryId;

                transaction.Store = null;
            }

            _dbSet.Add(transaction);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (transaction.Store != null)
                {
                    var retryStore = await _context.Stores
                        .FirstOrDefaultAsync(s => s.Name.ToLower() == transaction.Store.Name.Trim().ToLower());

                    if (retryStore != null)
                    {
                        transaction.Store = null;
                        transaction.StoreId = retryStore.Id;

                        // LUKA 5 ZAMKNIĘTA: Uzupełniamy kategorię podczas ponownej próby zapisu
                        transaction.CategoryId = retryStore.CategoryId;

                        await _context.SaveChangesAsync();
                    }
                    else throw;
                }
                else throw ex;
            }
        }
        public async Task<Store> GetOrCreateStoreAsync(string storeName)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Name.ToLower() == storeName.ToLower());
            if (store == null)
            {
                store = new Store { Name = storeName, CategoryId = 1 };
                _context.Stores.Add(store);
                await _context.SaveChangesAsync();
            }
            return store;
        }

        public async Task<IEnumerable<TransactionItem>> GetTransactionItemsAsync(int transactionId)
        {
            return await _context.Set<TransactionItem>()
                .Where(item => item.TransactionId == transactionId)
                .OrderByDescending(item => item.Quantity * item.UnitPrice)
                .AsNoTracking()
                .ToListAsync();

        }
    }
}