using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ExpenseControl.Data;
using ExpenseControl.DTOs;
using ExpenseControl.Models;
using ExpenseControl.Services.Interfaces;

namespace ExpenseControl.Services
{
    public class ReceiptService
    {
        private readonly IAIService _aiService;
        private readonly ApplicationDbContext _context;

        // Wstrzykujemy IAIService zamiast HttpClient i IConfiguration!
        public ReceiptService(IAIService aiService, ApplicationDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        public async Task<Transaction> AnalyzeReceiptAsync(Stream imageStream, string contentType)
        {
            // 1. Pobieramy kategorie i sklepy jako podpowiedź dla AI
            var categoriesList = await _context.Categories
                .Select(c => new { c.Id, c.Name })
                .AsNoTracking()
                .ToListAsync();
            var categoriesJson = JsonSerializer.Serialize(categoriesList);

            var storesList = await _context.Stores
                .Select(s => new { s.Id, s.Name, s.CategoryId })
                .AsNoTracking()
                .ToListAsync();
            var storesJson = JsonSerializer.Serialize(storesList);

            // 2. Budujemy Prompt
            var promptText = $@"
                Przeanalizuj paragon. Wyciągnij:
                - StoreName (Odczytana nazwa sklepu)
                - TotalAmount (Kwota łączna jako TEKST, np. ""15.50"")
    - TransactionDate (YYYY-MM-DD)
    
    DOPASOWANIE SKLEPU:
    Sprawdź, czy sklep z paragonu pasuje do któregoś z listy znanych sklepów: {storesJson}.
    - Jeśli pasuje: zwróć jego ID w polu 'StoreId'.
    - Jeśli NIE pasuje: zwróć null w 'StoreId'.

    DOPASOWANIE KATEGORII (Tylko jeśli StoreId jest null):
    - CategoryId: Wybierz ID kategorii z listy: {categoriesJson}.
    
    - Items: Lista produktów. Każdy produkt musi mieć:
      - Name (Nazwa produktu)
      - Quantity (Ilość jako TEKST, np. ""1"")
      - Price (Cena jednostkowa jako TEKST, np. ""5.99"")

    Zwróć TYLKO czysty JSON.";

            // 3. STRZAŁ DO AI (Czysto i elegancko!)
            // Cała logika Base64, HttpClienta i wycinania klamerek ukryta jest w GeminiService
            string cleanJson = "";
            try
            {
                cleanJson = await _aiService.AnalyzeImageAsync(imageStream, contentType, promptText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD Z API AI: {ex.Message}");
                return new Transaction { Store = new Store { Name = $"JSON: {cleanJson}" } };
            }

            // 4. Deserializacja do DTO
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            ReceiptDto dto;

            try
            {
                dto = JsonSerializer.Deserialize<ReceiptDto>(cleanJson, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD DESERIALIZACJI: {ex.Message}");
                Console.WriteLine($"JSON OD AI: {cleanJson}");
                return new Transaction { Store = new Store { Name = "Błąd parsowania JSON" } };
            }

            if (dto == null) return new Transaction();

            // 5. MAPOWANIE KOŃCOWE (Z DTO na Entity)
            var result = new Transaction();

            if (dto.StoreId.HasValue)
            {
                var matchedStore = storesList.FirstOrDefault(s => s.Id == dto.StoreId.Value);
                result.StoreId = dto.StoreId.Value;

                result.Store = new Store
                {
                    Id = dto.StoreId.Value,
                    Name = matchedStore?.Name ?? dto.StoreName,
                    CategoryId = matchedStore?.CategoryId ?? 1
                };
            }
            else
            {
                result.Store = new Store
                {
                    Name = dto.StoreName ?? "Nieznany Sklep",
                    CategoryId = dto.CategoryId ?? await GetDefaultCategoryId()
                };
            }

            result.TotalAmount = ParseDecimal(dto.TotalAmount);
            result.TransactionDate = DateTime.TryParse(dto.TransactionDate, out var d) ? d : DateTime.Now;

            if (dto.Items != null)
            {
                foreach (var itemDto in dto.Items)
                {
                    var qty = ParseDecimal(itemDto.Quantity);
                    result.Items.Add(new TransactionItem
                    {
                        Name = itemDto.Name ?? "Produkt",
                        Quantity = qty == 0 ? 1 : qty,
                        UnitPrice = ParseDecimal(itemDto.Price)
                    });
                }
            }

            return result;
        }

        // --- Metody pomocnicze zostają (bo one dotyczą danych/tekstu, a nie samego modelu AI) ---
        private async Task<int> GetDefaultCategoryId()
        {
            var defaultCat = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == "Inne");
            return defaultCat?.Id ?? 1;
        }

        private decimal ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            var clean = input.Replace("PLN", "", StringComparison.OrdinalIgnoreCase).Replace("zł", "", StringComparison.OrdinalIgnoreCase)
                .Replace("szt", "", StringComparison.OrdinalIgnoreCase).Replace("kg", "", StringComparison.OrdinalIgnoreCase)
                .Replace("l", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "").Trim().Replace(",", ".");
            return decimal.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0;
        }
    }
}