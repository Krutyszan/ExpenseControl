using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseControl.Models
{
    public class TransactionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        //Relacje
        [JsonIgnore]
        public Transaction Transaction { get; set; }
        public int TransactionId { get; set; }
    }
}
