using ExpenseControl.Data;
using ExpenseControl.Models.Interfaces;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseControl.Models
{
    public class Transaction : IEntity, IOwnedEntity
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        
        //Relacje
        public int StoreId { get; set; }
        public virtual Store? Store { get; set; }

        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
        public ICollection<Tag>? Tags { get; set; } = new HashSet<Tag>();

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; }

        //Pola Audytowe
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
