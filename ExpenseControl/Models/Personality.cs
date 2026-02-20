using ExpenseControl.Models.Interfaces;

namespace ExpenseControl.Models
{
    public class Personality : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PromptFragment { get; set; } = string.Empty;
    }
}
