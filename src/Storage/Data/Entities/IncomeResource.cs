namespace Storage.Data.Entities;

public class IncomeResource : IEntity
{
    public int Id { get; private set; }
    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;


    public decimal  Quantity { get; set; }

    public int ReceiptId { get; set; }
    public Receipt Receipt { get; set; } = null!;
}