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
        [JsonIgnore]
        public virtual ICollection<Transaction> Transactions { get; set; }
        public string Description { get; set; } = string.Empty;
        public string UserId { get ; set ; }
        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
