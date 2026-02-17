using Microsoft.AspNetCore.Mvc;
using ExpenseControl.Models;
using ExpenseControl.Services;

namespace ExpenseControl.Controllers
{
    //Route definiuje adres, kontroler zmieni się na TRANSACTIONS
    [Route("api/[controller]")]
    //Mówimy, że to API, a nie HTML
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionsService _service;

        public TransactionsController(TransactionsService service)
        {
            _service = service;
        }
        //Pobieramy transakcje
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _service.GetAllTransactionsAsync();

            return Ok(transactions);
        }

        //POST, dodajemy z JSONem

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Transaction transaction)
        {
            await _service.AddTransactionAsync(transaction);
            //Zwracamy 200 OK
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteTransactionAsync(id);

            return Ok();
        }

    }
}
