namespace FoodProduction.Models;

public class ProductionBatch
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public DateTime StartDate { get; set; }

    public decimal GainPercentage { get; set; }

    public decimal TotalCost { get; set; }

    public string? Notes { get; set; }

    public Product? Product { get; set; }

    public ICollection<Pack> Packs { get; set; } = [];
}
