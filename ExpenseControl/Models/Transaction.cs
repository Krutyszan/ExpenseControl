using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseControl.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        
        //Relacje
        public int StoreId { get; set; }
        public virtual Store? Store { get; set; }

        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();

        //Pola Audytowe
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
