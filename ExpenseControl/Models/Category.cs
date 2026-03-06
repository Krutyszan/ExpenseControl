using ExpenseControl.Data;
using ExpenseControl.Models.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseControl.Models
{
    public class Category : INamedEntity, IOwnedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; } = "#1e88e5";
        public string Emoji { get; set; } = "🏷️";
        [JsonIgnore]
        public virtual ICollection<Transaction> Transactions { get; set; }
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = default!;
        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; } = default!;

        //Subkategorie
        public int? ParentCategoryID { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category>? SubCategories { get; set; } = [];
    }
}
