namespace FoodProduction.Models;

public class ProductFormulation
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int IngredientId { get; set; }

    public decimal Ratio { get; set; } // Percentage (0-100)

    public Product? Product { get; set; }

    public Ingredient? Ingredient { get; set; }
}
