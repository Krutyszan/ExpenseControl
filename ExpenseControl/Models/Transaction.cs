namespace ExpenseControl.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
