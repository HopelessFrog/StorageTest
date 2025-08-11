namespace Storage.Data.Entities;

public class Receipt : IEntity
{
    public int Id { get; private set; }
    public string Number { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; }
    public List<IncomeResource> IncomeResources { get; set; } = new();
}