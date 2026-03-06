namespace ExpenseControl.DTOs
{
    public class ReceiptDto
    {
        public string StoreName { get; set; }
        public int? StoreId { get; set; }
        public string TransactionDate { get; set; }
        public List<ReceiptItemDto> Items { get; set; } = [];
    }
}
