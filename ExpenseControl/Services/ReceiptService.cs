using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExpenseControl.Models;
using ExpenseControl.DTOs;

namespace ExpenseControl.Services
{
    public class ReceiptService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public ReceiptService(IConfiguration configuration, HttpClient httpClient)
        {
            _apiKey = configuration["Gemini:ApiKey"];
            _httpClient = httpClient;
        }

        public async Task<Transaction> AnalyzeReceiptAsync(Stream imageStream, string contentType)
        {
            // 1. Konwersja obrazka na Base64 (bez zmian)
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            // 2. URL do Gemini 2.0 Flash (bez zmian)
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
            new
            {
                parts = new object[]
                {
                    // Tutaj instrukcja dla AI - prosimy o czysty JSON
                    new { text = "Przeanalizuj paragon. Zwróć JSON z polami: StoreName, TotalAmount, TransactionDate (YYYY-MM-DD), Category. TotalAmount zwróć jako samą liczbę w tekście, ale jeśli są waluty to je olej." },
                    new
                    {
                        inline_data = new
                        {
                            mime_type = contentType,
                            data = base64Image
                        }
                    }
                }
            }
        }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            // Obsługa błędów HTTP
            if (!response.IsSuccessStatusCode)
            {
                // ... (twoja obsługa błędu)
                return new Transaction();
            }

            var responseString = await response.Content.ReadAsStringAsync();

            // 3. Wyciąganie tekstu z odpowiedzi Google (to jest zagnieżdżone)
            using var doc = JsonDocument.Parse(responseString);
            var textResponse = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // 4. Czyszczenie Markdowna (AI lubi dodawać ```json)
            var cleanJson = textResponse
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            // 5. Deserializacja do DTO (Bezpieczna!)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            ReceiptDto dto;

            try
            {
                dto = JsonSerializer.Deserialize<ReceiptDto>(cleanJson, options);
            }
            catch
            {
                // Jak AI zwróci śmieci, zwracamy pusty obiekt, żeby nie wysadzić aplikacji
                return new Transaction { StoreName = "Błąd odczytu AI" };
            }

            if (dto == null) return new Transaction();

            // 6. MAPOWANIE: DTO (Stringi) -> TRANSACTION (Typy)
            // To jest najważniejsza część - tu naprawiamy błędy formatowania
            var result = new Transaction
            {
                StoreName = dto.StoreName ?? "Nieznany",
                Category = dto.Category ?? "Inne"
            };

            // Parsowanie Kwoty (TotalAmount)
            if (!string.IsNullOrEmpty(dto.TotalAmount))
            {
                // Usuwamy "PLN", "zł" i spacje
                var cleanAmount = dto.TotalAmount
                    .Replace("PLN", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("zł", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" ", "")
                    .Trim();

                // Zamieniamy przecinek na kropkę (dla pewności)
                cleanAmount = cleanAmount.Replace(",", ".");

                // Próbujemy parsować z kropką jako separatorem
                if (decimal.TryParse(cleanAmount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
                {
                    result.TotalAmount = amount;
                }
            }

            // Parsowanie Daty
            if (DateTime.TryParse(dto.TransactionDate, out var date))
            {
                result.TransactionDate = date;
            }
            else
            {
                result.TransactionDate = DateTime.Now;
            }

            return result;
        }
    }
}