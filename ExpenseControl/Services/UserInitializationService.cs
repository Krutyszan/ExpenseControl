using ExpenseControl.Data;
using ExpenseControl.Models;
using ExpenseControl.Services.Interfaces;

namespace ExpenseControl.Services
{
    public class UserInitializationService : IUserInitializationService
    {
        private readonly ApplicationDbContext _context;
        public UserInitializationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedDefaultUserDataAsync(string userId)
        {
            var defaultCategories = new List<Category>
            {
                new Category{ Name = "Inne", Description = "Wydatki niesklasyfikowane", UserId = userId},
                new Category{ Name = "Jedzenie", Description = "Spożywcze i restauracje", UserId = userId},
                new Category{ Name = "Transport", Description = "Paliwo, bilety", UserId = userId},
                new Category{ Name = "Rozrywka", Description = "Gry, kino, zabawki", UserId=userId}
            };

            var lidl = new Store { Name = "Lidl", Category = defaultCategories[1], UserId = userId };
            var auchan = new Store { Name = "Auchan", Category = defaultCategories[1], UserId = userId };
            var biedronka = new Store { Name = "Biedronka", Category = defaultCategories[1], UserId = userId };
            var zabka = new Store { Name = "Żabka", Category = defaultCategories[1], UserId = userId };
            var orlen = new Store { Name = "Orlen", Category = defaultCategories[2], UserId = userId };
            var steam = new Store { Name = "Steam", Category = defaultCategories[3], UserId = userId };

            // Dodajemy wszystko do kontekstu
            await _context.Categories.AddRangeAsync(defaultCategories);
            await _context.Stores.AddRangeAsync(biedronka, orlen, steam);

            // Zapisujemy do bazy jednym uderzeniem
            await _context.SaveChangesAsync();
        }
    }
}
