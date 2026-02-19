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

        // 1. ZMIANA: Dodajemy .Include, żeby widzieć nazwę sklepu, kategorie i produkty w tabeli
        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Store)              // Dociągnij Sklep
                    .ThenInclude(s => s.Category)   // Dociągnij Kategorię Sklepu
                .Include(t => t.Items)              // Dociągnij Listę Zakupów
                .OrderByDescending(t => t.TransactionDate)
                .AsNoTracking()
                .ToListAsync();
        }

        // 2. ZMIANA: Logika "Sklejania" relacji przy dodawaniu
        public async Task AddTransactionAsync(Transaction transaction)
        {
            // 1. Logika SKLEPU (Pancerna)
            if (transaction.Store != null && !string.IsNullOrWhiteSpace(transaction.Store.Name))
            {
                // Czyścimy nazwę ze spacji (to najczęstszy powód błędu!)
                var cleanName = transaction.Store.Name.Trim();
                transaction.Store.Name = cleanName; // Aktualizujemy w obiekcie

                // Sprawdzamy czy sklep istnieje (ignorując wielkość liter)
                var existingStore = await _context.Stores
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == cleanName.ToLower());

                if (existingStore != null)
                {
                    // SCENARIUSZ A: Sklep istnieje.
                    // Podpinamy transakcję pod ID istniejącego sklepu.
                    transaction.StoreId = existingStore.Id;

                    // CRITICAL FIX: Musimy wyzerować obiekt Store!
                    // Jeśli tego nie zrobimy, EF Core pomyśli: "To jest nowy obiekt w pamięci, dodam go".
                    transaction.Store = null;
                }
                else
                {
                    // SCENARIUSZ B: Sklep jest faktycznie nowy.
                    // EF go doda. Upewniamy się tylko co do kategorii.
                    if (transaction.Store.CategoryId == 0)
                    {
                        var defaultCat = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Inne");
                        transaction.Store.CategoryId = defaultCat?.Id ?? 1;
                    }
                }
            }
            // Zabezpieczenie: Jeśli user nie wpisał sklepu lub coś poszło nie tak
            else if (transaction.StoreId == 0)
            {
                var unknownStore = await GetOrCreateStoreAsync("Nieznany Sklep");
                transaction.StoreId = unknownStore.Id;
                transaction.Store = null; // Tu też czyścimy dla bezpieczeństwa
            }

            // 2. Dodajemy transakcję
            _context.Transactions.Add(transaction);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Jeśli jakimś cudem (np. wyścig wątków) to nadal wywali błąd Unique,
                // to znaczy, że sklep został dodany ułamek sekundy temu.
                // Wtedy robimy ostateczny fallback: pobieramy ID jeszcze raz.
                if (transaction.Store != null)
                {
                    var retryStore = await _context.Stores
                        .FirstOrDefaultAsync(s => s.Name.ToLower() == transaction.Store.Name.Trim().ToLower());

                    if (retryStore != null)
                    {
                        transaction.Store = null;
                        transaction.StoreId = retryStore.Id;
                        await _context.SaveChangesAsync();
                    }
                    else throw; // Jak nadal nie działa, to poddajemy się
                }
                else throw ex;
            }
        }

        // Pomocnicza metoda (przydaje się czasem)
        public async Task<Store> GetOrCreateStoreAsync(string storeName)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Name.ToLower() == storeName.ToLower());
            if (store == null)
            {
                store = new Store { Name = storeName, CategoryId = 1 }; // Domyślna kategoria
                _context.Stores.Add(store);
                await _context.SaveChangesAsync();
            }
            return store;
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}