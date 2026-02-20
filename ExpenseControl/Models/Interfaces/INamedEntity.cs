namespace ExpenseControl.Models.Interfaces
{
    public interface INamedEntity : IEntity
    {
        string Name { get; }
    }
}
