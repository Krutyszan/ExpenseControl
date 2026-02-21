using ExpenseControl.Data;
using ExpenseControl.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseControl.Models
{
    public class TransactionItem : INamedEntity, IOwnedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        //Relacje
        [JsonIgnore]
        public Transaction Transaction { get; set; }
        public int TransactionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
