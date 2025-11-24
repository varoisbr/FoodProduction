namespace FoodProduction.Models;

public class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public decimal DefaultGainPercentage { get; set; } = 0;

    public int? DefaultLabelTemplateId { get; set; }

    public LabelTemplate? DefaultLabelTemplate { get; set; }

    public ICollection<ProductFormulation> Formulations { get; set; } = [];

    public ICollection<ProductionBatch> ProductionBatches { get; set; } = [];
}
