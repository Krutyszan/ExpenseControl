using System.Text.Json.Serialization;

namespace ExpenseControl.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public virtual ICollection<Transaction> Transactions { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
