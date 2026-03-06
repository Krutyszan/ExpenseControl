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
        [ForeignKey("CategoryId")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        [JsonIgnore]
        public Transaction Transaction { get; set; }
        public int TransactionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
