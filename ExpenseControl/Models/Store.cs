using ExpenseControl.Data;
using ExpenseControl.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseControl.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Store : INamedEntity, IOwnedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;

        //Relacje
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        [JsonIgnore]
        public virtual ICollection<Transaction> Transactions { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; }

    }
}
