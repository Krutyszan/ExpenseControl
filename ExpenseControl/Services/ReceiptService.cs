using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ExpenseControl.Data;
using ExpenseControl.DTOs;
using ExpenseControl.Models;

namespace ExpenseControl.Services
{
    public class ReceiptService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public ReceiptService(IConfiguration configuration, HttpClient httpClient, ApplicationDbContext context)
        {
            _apiKey = configuration["Gemini:ApiKey"];
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<Transaction> AnalyzeReceiptAsync(Stream imageStream, string contentType)
        {
            // 1a. Pobieramy kategorie
            var categoriesList = await _context.Categories
                .Select(c => new { c.Id, c.Name })
                .AsNoTracking()
                .ToListAsync();
            var categoriesJson = JsonSerializer.Serialize(categoriesList);

            // 1b. Pobieramy listę sklepów (Twoja nowa wstawka)
            var storesList = await _context.Stores
                .Select(s => new { s.Id, s.Name, s.CategoryId }) // Pobieramy też CategoryId sklepu, przyda się!
                .AsNoTracking()
                .ToListAsync();
            var storesJson = JsonSerializer.Serialize(storesList);

            // 2. Obrazek -> Base64
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            // 3. Prompt (Zaktualizowany o Stores)
            var promptText = $@"
                Przeanalizuj paragon. Wyciągnij:
                - StoreName (Odczytana nazwa sklepu)
                - TotalAmount (Kwota łączna - liczba)
                - TransactionDate (YYYY-MM-DD)
                
                DOPASOWANIE SKLEPU:
                Sprawdź, czy sklep z paragonu pasuje do któregoś z listy znanych sklepów: {storesJson}.
                - Jeśli pasuje: zwróć jego ID w polu 'StoreId'.
                - Jeśli NIE pasuje: zwróć null w 'StoreId'.

                DOPASOWANIE KATEGORII (Tylko jeśli StoreId jest null):
                - CategoryId: Wybierz ID kategorii z listy: {categoriesJson}.
                
                - TransactionItems: Lista produktów (Name, Quantity, Price).

                Zwróć TYLKO czysty JSON.";

            // 4. Payload
            var payload = CreatePayload(promptText, base64Image, contentType);
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // 5. Strzał do API
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Błąd API Gemini: {response.StatusCode} - {errorMsg}");
            }

            // 6-8. Parsowanie (Standard)
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var textResponse = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            var cleanJson = textResponse.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            ReceiptDto dto;
            try { dto = JsonSerializer.Deserialize<ReceiptDto>(cleanJson, options); }
            catch { return new Transaction { Store = new Store { Name = "Błąd odczytu AI" } }; }

            if (dto == null) return new Transaction();

            // 9. MAPOWANIE KOŃCOWE (Tu jest magia)
            var result = new Transaction();

            // A) AI rozpoznało istniejący sklep
            if (dto.StoreId.HasValue)
            {
                // Szukamy tego sklepu w naszej lokalnej liście, żeby wyciągnąć jego ładną nazwę i kategorię
                var matchedStore = storesList.FirstOrDefault(s => s.Id == dto.StoreId.Value);

                result.StoreId = dto.StoreId.Value; // Wiążemy po ID (dla backendu)

                // Tworzymy obiekt Store tylko dla UI (żeby w formularzu user widział "Biedronka", a nie puste pole)
                result.Store = new Store
                {
                    Id = dto.StoreId.Value,
                    Name = matchedStore?.Name ?? dto.StoreName, // Bierzemy nazwę z bazy (ładniejszą)
                    CategoryId = matchedStore?.CategoryId ?? 1
                };
            }
            // B) AI nie znalazło sklepu (Nowy sklep)
            else
            {
                result.Store = new Store
                {
                    Name = dto.StoreName ?? "Nieznany Sklep",
                    CategoryId = dto.CategoryId ?? await GetDefaultCategoryId()
                };
            }

            // Reszta pól (Kwota, Data, Items)
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
                        PricePerUnit = ParseDecimal(itemDto.PricePerUnit)
                    });
                }
            }

            return result;
        }

        // --- Metody pomocnicze bez zmian ---
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

        private object CreatePayload(string prompt, string base64Data, string mimeType)
        {
            return new { contents = new[] { new { parts = new object[] { new { text = prompt }, new { inline_data = new { mime_type = mimeType, data = base64Data } } } } } };
        }
    }
}