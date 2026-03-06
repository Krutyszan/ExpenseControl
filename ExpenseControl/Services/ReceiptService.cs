using ExpenseControl.Data;
using ExpenseControl.DTOs;
using ExpenseControl.Models;
using ExpenseControl.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ExpenseControl.Services
{
    public class ReceiptService
    {
        private readonly IAIService _aiService;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ReceiptService(IAIService aiService, ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _aiService = aiService;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ReceiptDto> AnalyzeReceiptAsync(Stream imageStream, string contentType)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Brak zalogowanego użytkownika. Zaloguj się ponownie.");

            var categoriesList = await _context.Categories.AsNoTracking().Select(c => new { c.Id, c.Name }).ToListAsync();
            var storesList = await _context.Stores.AsNoTracking().Select(s => new { s.Id, s.Name, s.DefaultCategoryId }).ToListAsync();

            var categoriesJson = JsonSerializer.Serialize(categoriesList);
            var storesJson = JsonSerializer.Serialize(storesList);

            var promptText = $@"
                Przeanalizuj paragon i zwróć JSON.
                DANE: Sklepy: {storesJson}, Kategorie: {categoriesJson}.
                ZASADY: 
                - Jeśli sklep pasuje do listy, daj jego 'StoreId'. Jeśli nie, daj null.
                - Dopasuj 'CategoryId'. Jeśli nie wiesz, daj DefaultCategoryId sklepu lub ID kategorii 'Inne'.
                
                Zwróć JSON:
                {{
                    ""StoreName"": ""nazwa"",
                    ""StoreId"": int?,
                    ""TransactionDate"": ""YYYY-MM-DD"",
                    ""Items"": [ {{ ""Name"": """", ""Price"": """", ""Quantity"": """", ""CategoryId"": int }} ]
                }}";

            var cleanJson = await _aiService.AnalyzeImageAsync(imageStream, contentType, promptText);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<ReceiptDto>(cleanJson, options);

            if (dto == null)
                throw new InvalidOperationException("AI zwróciło puste dane lub błędny JSON.");

            return dto;
        }

        public async Task<Transaction> SaveReceiptFromDtoAsync(ReceiptDto dto)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("Utracono sesję użytkownika.");

            int finalStoreId;
            if (dto.StoreId.HasValue && dto.StoreId.Value > 0)
            {
                finalStoreId = dto.StoreId.Value;
            }
            else
            {
                var existingStore = await _context.Stores
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == (dto.StoreName ?? "").ToLower());

                if (existingStore != null)
                {
                    finalStoreId = existingStore.Id;
                }
                else
                {
                    // Tworzymy nowy sklep
                    var newStore = new Store
                    {
                        Name = string.IsNullOrWhiteSpace(dto.StoreName) ? "Nowy Sklep" : dto.StoreName,
                        UserId = userId,
                        DefaultCategoryId = await GetDefaultCategoryId()
                    };
                    _context.Stores.Add(newStore);
                    await _context.SaveChangesAsync();
                    finalStoreId = newStore.Id;
                }
            }

            // 2. Transakcja
            var transaction = new Transaction
            {
                UserId = userId,
                StoreId = finalStoreId,
                Store = null, // NULL jest kluczowy!
                TransactionDate = DateTime.TryParse(dto.TransactionDate, out var dt) ? dt : DateTime.Now,
                Items = new List<TransactionItem>()
            };

            // 3. Produkty
            var defaultCatId = await GetDefaultCategoryId();
            foreach (var itemDto in dto.Items)
            {
                transaction.Items.Add(new TransactionItem
                {
                    Name = itemDto.Name,
                    Quantity = ParseDecimal(itemDto.Quantity) == 0 ? 1 : ParseDecimal(itemDto.Quantity),
                    UnitPrice = ParseDecimal(itemDto.Price),
                    CategoryId = itemDto.CategoryId ?? defaultCatId
                });
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        // Metody pomocnicze
        private async Task<int> GetDefaultCategoryId()
        {
            var cat = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Inne");
            return cat?.Id ?? 1;
        }

        private decimal ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            var clean = Regex.Replace(input, @"[^\d.,]", "").Replace(",", ".");
            return decimal.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var res) ? res : 0;
        }
    }
}