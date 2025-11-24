namespace FoodProduction.Models;

public class Pack
{
    public int Id { get; set; }

    public int BatchId { get; set; }

    public decimal WeightKg { get; set; }

    public decimal Price { get; set; }

    public bool Printed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProductionBatch? Batch { get; set; }
}
