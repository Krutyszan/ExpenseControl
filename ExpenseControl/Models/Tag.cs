using ExpenseControl.Data;
using ExpenseControl.Models.Interfaces;

namespace ExpenseControl.Models
{
    public class Tag : IEntity, IOwnedEntity, INamedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ColorHex { get; set; } = "#4DB322";
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }
    }
}
