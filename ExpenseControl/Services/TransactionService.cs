using ExpenseControl.Data;
using ExpenseControl.Extensions;
using ExpenseControl.Models;
using ExpenseControl.Services.Base;
using ExpenseControl.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
                .Include(t => t.Items)
                    .ThenInclude(i => i.Category)
                .Include(t => t.Tags)
                .OrderByDescending(t => t.TransactionDate)
                .AsNoTracking()
                .ToListAsync();
        }
        public override async Task AddAsync(Transaction transaction)
        {
            // Zmienna, która przechowa ostateczną decyzję co do kategorii
            int resolvedCategoryId;

            // ---------------------------------------------------------
            // KROK 1: LOGIKA SKLEPU
            // ---------------------------------------------------------
            if (transaction.Store != null && !string.IsNullOrWhiteSpace(transaction.Store.Name))
            {
                var cleanName = transaction.Store.Name.Trim();
                transaction.Store.Name = cleanName;

                var existingStore = await _context.Stores
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == cleanName.ToLower());

                if (existingStore != null)
                {
                    // SCENARIUSZ A: Sklep istnieje
                    transaction.StoreId = existingStore.Id;
                    transaction.Store = null; // Odpinamy, żeby nie dublować

                    // Proste przypisanie - to nie jest nullowalne, więc bierzemy jak leci
                    // Zakładamy, że w bazie istniejące sklepy mają poprawne ID > 0
                    resolvedCategoryId = existingStore.DefaultCategoryId;
                }
                else
                {
                    // SCENARIUSZ B: Nowy sklep (AI lub ręcznie wpisany)
                    // Sprawdzamy, czy przyszło jakieś ID (np. wybrane z listy). 
                    // Jeśli jest 0, to znaczy, że nie wybrano nic -> dajemy "Inne".
                    if (transaction.Store.DefaultCategoryId == 0)
                    {
                        resolvedCategoryId = await _context.GetCategoryIdByNameAsync("Inne");
                        transaction.Store.DefaultCategoryId = resolvedCategoryId; // Ustawiamy dla nowego sklepu
                    }
                    else
                    {
                        resolvedCategoryId = transaction.Store.DefaultCategoryId;
                    }
                }
            }
            else
            {
                // SCENARIUSZ C: Brak nazwy sklepu (Nieznany)
                // Jeśli nie podałeś ID (0), to uznajemy to za "Nieznany Sklep"
                if (transaction.StoreId == 0)
                {
                    var unknownStore = await GetOrCreateStoreAsync("Nieznany Sklep");
                    transaction.StoreId = unknownStore.Id;
                    transaction.Store = null;
                    resolvedCategoryId = unknownStore.DefaultCategoryId;
                }
                else
                {
                    // Ktoś podał samo ID sklepu (np. wybrał z listy, ale nie załadował obiektu Store)
                    // Pobieramy kategorię tego sklepu "na szybko"
                    var storeShort = await _context.Stores
                        .Where(s => s.Id == transaction.StoreId)
                        .Select(s => new { s.DefaultCategoryId })
                        .FirstOrDefaultAsync();

                    resolvedCategoryId = storeShort?.DefaultCategoryId ?? await _context.GetCategoryIdByNameAsync("Inne");
                }
            }


            if (transaction.Items == null || !transaction.Items.Any())
            {
                transaction.Items = new List<TransactionItem>
        {
            new TransactionItem
            {
                Name = transaction.Store?.Name ?? "Zakupy",
                Quantity = 1,                
                CategoryId = resolvedCategoryId,
                UserId = transaction.UserId
            }
        };
            }
            else
            {

                foreach (var item in transaction.Items)
                {
                    if (item.CategoryId == 0)
                    {
                        item.CategoryId = resolvedCategoryId;
                    }
                }
            }

            _dbSet.Add(transaction);
            await _context.SaveChangesAsync();
        }
        public async Task<Store> GetOrCreateStoreAsync(string storeName)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Name.ToLower() == storeName.ToLower());
            if (store == null)
            {
                store = new Store { Name = storeName, DefaultCategoryId = 1 };
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
        public async Task<IEnumerable<TransactionItem>> GetGroupedTransactionItemsAsync(int transactionId)
        {
            return await _context.Set<TransactionItem>()
                .Where(item => item.TransactionId == transactionId)
                .OrderByDescending(item => item.Quantity * item.UnitPrice)
                .AsNoTracking()
                .ToListAsync();

        }

    }
}