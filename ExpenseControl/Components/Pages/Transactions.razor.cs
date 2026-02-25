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
        [Inject] private ICategoryService CategoryService { get; set; } = default!;
        [Inject] private ReceiptService ReceiptService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] public ICurrentUserService CurrentUserService { get; set; } = default!;

        // Uchwyt do naszego generycznego komponentu HTML
        private BaseCrud<Transaction> _crud = default!;

        private IEnumerable<Category> _categories = new List<Category>();
        private bool _isScanning = false;

        protected override async Task OnInitializedAsync()
        {
            if (CategoryService != null)
            {
                _categories = await CategoryService.GetAllAsync();
            }
        }

        private async Task UploadFiles(IBrowserFile file)
        {
            if (file == null) return;

            _isScanning = true;
            StateHasChanged();

            try
            {
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                var aiTransaction = await ReceiptService.AnalyzeReceiptAsync(stream, file.ContentType);

                await Service.AddAsync(aiTransaction);
                Snackbar.Add($"Zapisano paragon ze sklepu {aiTransaction.Store?.Name} na kwotę {aiTransaction.TotalAmount} PLN", Severity.Success);

                // Magia: zmuszamy komponent BaseCrud do pobrania świeżych danych z bazy!
                if (_crud != null)
                {
                    await _crud.RefreshAsync();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Błąd AI: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isScanning = false;
                StateHasChanged();
            }
        }
    }
}