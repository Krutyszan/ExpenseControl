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
            Console.WriteLine($"[KOMITET POWITALNY] Rozpoczynam tworzenie danych dla: {userId}");
            var defaultCategories = new List<Category>

            {
                new Category { Name = "Dom i Rachunki", Emoji = "🏠", ColorHex = "#3F51B5", UserId= userId }, // Indigo
                new Category { Name = "Jedzenie", Emoji = "🍖", ColorHex = "#4CAF50", UserId= userId }, // Green
                new Category { Name = "Transport", Emoji = "🚗", ColorHex = "#607D8B", UserId= userId }, // BlueGrey
                new Category { Name = "Zdrowie", Emoji = "💊", ColorHex = "#E91E63", UserId= userId }, // Pink
                new Category { Name = "Hobby", Emoji = "🎨", ColorHex = "#FF9800", UserId= userId }, // Orange
                new Category { Name = "Ubrania", Emoji = "🛍️", ColorHex = "#9C27B0", UserId= userId }, // Purple
                new Category { Name = "Rozrywka", Emoji = "🎮", ColorHex = "#00BCD4", UserId= userId }, // Cyan
                new Category { Name = "Chemia", Emoji = "🧴", ColorHex = "#8BC34A", UserId= userId }, // LightGreen
                new Category { Name = "Kosmetyki", Emoji = "💄", ColorHex = "#FF5722", UserId= userId }, // DeepOrange
                new Category { Name = "Edukacja", Emoji = "📚", ColorHex = "#FFEB3B", UserId= userId }, // Yellow
                new Category { Name = "Prezenty", Emoji = "🎁", ColorHex = "#F44336", UserId= userId }, // Red
                new Category { Name = "Elektronika", Emoji = "💻", ColorHex = "#212121", UserId= userId }  // Black/Dark
            };
            var stores = new List<Store>
            {
                // --- JEDZENIE [1] ---
                new Store { Name = "Biedronka", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Lidl", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Żabka", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Kaufland", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Auchan", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Carrefour", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Dino", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Galeria Wypieków Lubaszka", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Piekarnia", DefaultCategory = defaultCategories[1], UserId = userId },
                new Store { Name = "Warzywniak", DefaultCategory = defaultCategories[1], UserId = userId },

                // --- TRANSPORT [2] ---
                new Store { Name = "Orlen", DefaultCategory = defaultCategories[2], UserId = userId },
                new Store { Name = "BP", DefaultCategory = defaultCategories[2], UserId = userId },
                new Store { Name = "Shell", DefaultCategory = defaultCategories[2], UserId = userId },
                new Store { Name = "Circle K", DefaultCategory = defaultCategories[2], UserId = userId },
                new Store { Name = "Uber", DefaultCategory = defaultCategories[2], UserId = userId },
                new Store { Name = "Bolt", DefaultCategory = defaultCategories[2], UserId = userId },
                new Store { Name = "PKP Intercity", DefaultCategory = defaultCategories[2], UserId = userId },

                // --- DOM I RACHUNKI [0] ---
                new Store { Name = "IKEA", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Castorama", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Leroy Merlin", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Obi", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Czynsz", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "PGE", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "UPC", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Play", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Netia", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Jysk", DefaultCategory = defaultCategories[0], UserId = userId },
                new Store { Name = "Pepco", DefaultCategory = defaultCategories[0], UserId = userId }, // Często rzeczy do domu

                // --- ZDROWIE [3] ---
                new Store { Name = "Apteka DOZ", DefaultCategory = defaultCategories[3], UserId = userId },
                new Store { Name = "Apteka Gemini", DefaultCategory = defaultCategories[3], UserId = userId },
                new Store { Name = "LuxMed", DefaultCategory = defaultCategories[3], UserId = userId },
                new Store { Name = "Medicover", DefaultCategory = defaultCategories[3], UserId = userId },
                new Store { Name = "Stomatolog", DefaultCategory = defaultCategories[3], UserId = userId },

                // --- KOSMETYKI [8] i CHEMIA [7] ---
                new Store { Name = "Rossmann", DefaultCategory = defaultCategories[8], UserId = userId },
                new Store { Name = "Hebe", DefaultCategory = defaultCategories[8], UserId = userId },
                new Store { Name = "Super-Pharm", DefaultCategory = defaultCategories[8], UserId = userId },
                new Store { Name = "Sephora", DefaultCategory = defaultCategories[8], UserId = userId },
                new Store { Name = "Douglas", DefaultCategory = defaultCategories[8], UserId = userId },
                new Store { Name = "Dealz", DefaultCategory = defaultCategories[7], UserId = userId }, // Często chemia domowa
                new Store { Name = "Action", DefaultCategory = defaultCategories[7], UserId = userId }, // Często chemia/dom

                // --- UBRANIA [5] ---
                new Store { Name = "Zalando", DefaultCategory = defaultCategories[5], UserId = userId },
                new Store { Name = "H&M", DefaultCategory = defaultCategories[5], UserId = userId },
                new Store { Name = "Zara", DefaultCategory = defaultCategories[5], UserId = userId },
                new Store { Name = "Reserved", DefaultCategory = defaultCategories[5], UserId = userId },
                new Store { Name = "Vinted", DefaultCategory = defaultCategories[5], UserId = userId },
                new Store { Name = "CCC", DefaultCategory = defaultCategories[5], UserId = userId },
                new Store { Name = "Eobuwie", DefaultCategory = defaultCategories[5], UserId = userId },
                new Store { Name = "Sinsay", DefaultCategory = defaultCategories[5], UserId = userId },

                // --- ELEKTRONIKA [11] ---
                new Store { Name = "Media Expert", DefaultCategory = defaultCategories[11], UserId = userId },
                new Store { Name = "RTV Euro AGD", DefaultCategory = defaultCategories[11], UserId = userId },
                new Store { Name = "Media Markt", DefaultCategory = defaultCategories[11], UserId = userId },
                new Store { Name = "X-Kom", DefaultCategory = defaultCategories[11], UserId = userId },
                new Store { Name = "Morele", DefaultCategory = defaultCategories[11], UserId = userId },
                new Store { Name = "Apple", DefaultCategory = defaultCategories[11], UserId = userId },

                // --- ROZRYWKA [6] i HOBBY [4] ---
                new Store { Name = "Netflix", DefaultCategory = defaultCategories[6], UserId = userId },
                new Store { Name = "Spotify", DefaultCategory = defaultCategories[6], UserId = userId },
                new Store { Name = "Steam", DefaultCategory = defaultCategories[6], UserId = userId },
                new Store { Name = "Cinema City", DefaultCategory = defaultCategories[6], UserId = userId },
                new Store { Name = "Multikino", DefaultCategory = defaultCategories[6], UserId = userId },
                new Store { Name = "Empik", DefaultCategory = defaultCategories[4], UserId = userId }, // Hobby/Książki
                new Store { Name = "Decathlon", DefaultCategory = defaultCategories[4], UserId = userId }, // Hobby/Sport
                new Store { Name = "Allegro", DefaultCategory = defaultCategories[0], UserId = userId }, // Allegro to wszystko, ale dałem Dom jako bezpieczny start
                new Store { Name = "Amazon", DefaultCategory = defaultCategories[0], UserId = userId }
            };

            await _context.Categories.AddRangeAsync(defaultCategories);
            await _context.Stores.AddRangeAsync(stores);
            await _context.SaveChangesAsync();
        }
    }
}
