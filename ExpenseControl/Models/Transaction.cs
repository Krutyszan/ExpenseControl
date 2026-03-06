using ExpenseControl.Data;
using ExpenseControl.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseControl.Models
{
    public class Transaction : IEntity, IOwnedEntity
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
        //Relacje
        public int StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; } = default!;
        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
        public ICollection<Tag>? Tags { get; set; } = new HashSet<Tag>();

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; } = default!;

        //Pola Audytowe
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
