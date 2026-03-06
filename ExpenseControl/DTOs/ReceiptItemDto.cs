namespace ExpenseControl.DTOs
{
    public class ReceiptItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
    }
}
