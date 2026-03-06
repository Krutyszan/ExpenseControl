using ExpenseControl.Components.Base;
using ExpenseControl.Models;
using ExpenseControl.Services;
using ExpenseControl.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace ExpenseControl.Components.Pages
{
    public partial class Transactions : ComponentBase
    {
        [Inject] private ITransactionService Service { get; set; } = default!;
        [Inject] private ITransactionItemService ItemService { get; set; } = default!;
        [Inject] private ICategoryService CategoryService { get; set; } = default!;
        [Inject] private ReceiptService ReceiptService { get; set; } = default!; // Twój nowy serwis
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] public ICurrentUserService CurrentUserService { get; set; } = default!;
        [Inject] public IDialogService DialogService { get; set; } = default!; // Potrzebny do Twojego dialogu
        [Inject] public ITagService TagsService { get; set; } = default!;

        private BaseCrud<Transaction>? _crud;

        private IEnumerable<Category> _categories = new List<Category>();
        private bool _isScanning = false;
        private IEnumerable<Tag> _tags = new List<Tag>();

        protected override async Task OnInitializedAsync()
        {
            // Pobieramy dane potrzebne do dropdownów w dialogu
            _categories = await CategoryService.GetAllAsync();
            _tags = await TagsService.GetAllAsync();
        }

        // --- FUNKCJA 1: SKANOWANIE PARAGONU ---
        private async Task UploadFiles(IBrowserFile file)
        {
            if (file == null) return;

            _isScanning = true;
            StateHasChanged(); // Włączamy spinner

            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // Limit 10MB

                // KROK A: Analiza przez AI (zwraca DTO)
                var receiptDto = await ReceiptService.AnalyzeReceiptAsync(stream, file.ContentType);

                // KROK B: Bezpieczny zapis do bazy (naprawia sklepy i UserID)
                var savedTransaction = await ReceiptService.SaveReceiptFromDtoAsync(receiptDto);

                Snackbar.Add($"Zapisano paragon: {savedTransaction.Store?.Name ?? "Sklep"} ({savedTransaction.TotalAmount:C2})", Severity.Success);

                // Odświeżamy tabelę
                if (_crud != null)
                {
                    await _crud.RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                // Wyciągamy prawdziwą przyczynę błędu
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"BŁĄD SKANOWANIA: {ex}");
                Snackbar.Add($"Błąd: {errorMsg}", Severity.Error);
            }
            finally
            {
                _isScanning = false;
                StateHasChanged(); // Wyłączamy spinner
            }
        }

        // --- FUNKCJA 2: EDYCJA POZYCJI (Używa Twojego TransactionItemDialog) ---
        private async Task EditItemAsync(TransactionItem item)
        {
            // 1. Tworzymy kopię obiektu. 
            // To bardzo ważne! Gdybyśmy edytowali oryginał, zmiany w tabeli pojawiłyby się 
            // zanim klikniesz "Zapisz", a przycisku "Anuluj" nie dałoby się obsłużyć.
            var itemCopy = new TransactionItem
            {
                Id = item.Id,
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                CategoryId = item.CategoryId,
                TransactionId = item.TransactionId,
                UserId = item.UserId,
            };

            // 2. Przygotowujemy parametry zgodne z Twoim dialogiem
            var parameters = new DialogParameters
            {
                { "Model", itemCopy },       // Przekazujemy kopię
                { "Categories", _categories } // Przekazujemy listę kategorii pobraną w OnInitialized
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

            // 3. Wywołujemy Twój dialog
            var dialog = await DialogService.ShowAsync<TransactionItemDialog>("Edytuj pozycję", parameters, options);
            var result = await dialog.Result;

            // 4. Jeśli użytkownik kliknął "Zapisz" (nie anulował)
            if (!result.Canceled && result.Data is TransactionItem updatedItem)
            {
                try
                {
                    // Czyścimy obiekt nawigacyjny Category, żeby EF Core nie próbował go śledzić/zmieniać
                    // Interesuje nas tylko zmiana CategoryId
                    updatedItem.Category = null;

                    await ItemService.UpdateAsync(updatedItem);

                    Snackbar.Add("Zaktualizowano pozycję", Severity.Success);

                    if (_crud != null)
                    {
                        await _crud.RefreshAsync();
                    }
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Błąd zapisu: {ex.Message}", Severity.Error);
                }
            }
        }

        // --- FUNKCJA 3: USUWANIE ---
        private async Task DeleteItemAsync(int itemId)
        {
            try
            {
                await ItemService.DeleteAsync(itemId);
                Snackbar.Add("Pozycja usunięta", Severity.Success);
                if (_crud != null) await _crud.RefreshAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd usuwania: {ex.Message}", Severity.Error);
            }
        }
    }
}