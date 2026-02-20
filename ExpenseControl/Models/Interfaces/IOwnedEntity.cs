using ExpenseControl.Data;

namespace ExpenseControl.Models.Interfaces
{
    public interface IOwnedEntity : IEntity
    {
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
