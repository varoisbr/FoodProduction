namespace FoodProduction.Models;

public class Ingredient
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public decimal CostPerKg { get; set; }

    public ICollection<ProductFormulation> ProductFormulations { get; set; } = [];
}
