using ExpenseControl.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace ExpenseControl.Services
{
    public class GeminiService : IAIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration config, HttpClient httpClient)
        {
            _apiKey = config["Gemini:ApiKey"];
            _httpClient = httpClient;
        }

        public async Task<string> AnalyzeImageAsync(Stream imageStream, string contentType, string prompt)
        {
            //Zamiana strumienia na bajty
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            //payload
            var payload = CreatePayload(prompt, base64Image, contentType);
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            //Wysyłka
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Błąd API Gemini: {response.StatusCode} - {errorMsg}");
            }

            //Parsowanie
            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var textResponse = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

            var cleanJson = textResponse ?? "";

            // 1. Zdejmujemy ewentualne formatowanie Markdown, które Gemini uwielbia dodawać
            cleanJson = cleanJson.Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                                 .Replace("```", "")
                                 .Trim();

            // 2. Szukamy pierwszego otwarcia i ostatniego zamknięcia (obiektu LUB tablicy)
            // IndexOfAny to potężna metoda C#, która znajdzie pierwszą klamerkę { LUB nawias [
            var startIndex = cleanJson.IndexOfAny(new[] { '{', '[' });
            var endIndex = cleanJson.LastIndexOfAny(new[] { '}', ']' });

            if (startIndex >= 0 && endIndex > startIndex)
            {
                // Wycinamy samo gęste - czysty JSON bez żadnego tekstu pobocznego
                cleanJson = cleanJson.Substring(startIndex, endIndex - startIndex + 1);
            }
            else
            {
                // Jeśli AI wypluło całkowitą bzdurę, rzucamy błąd z dokładnym tekstem od modelu.
                // Twój ReceiptService złapie to i wyświetli ładnie w interfejsie!
                throw new Exception($"AI nie zwróciło JSON-a! Zwróciło: {textResponse}");
            }

            return cleanJson;
        }

        public Task<string> GenerateTextAsync(string prompt)
        {
            throw new NotImplementedException();
        }

        private object CreatePayload(string prompt, string base64Data, string mimeType)
        {
            return new { contents = new[] { new { parts = new object[] { new { text = prompt }, new { inline_data = new { mime_type = mimeType, data = base64Data } } } } } };
        }
    }
}
