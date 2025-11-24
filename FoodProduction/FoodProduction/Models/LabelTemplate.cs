namespace FoodProduction.Models;

public class LabelTemplate
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Content { get; set; } // ZPL code with placeholders: {ProductName}, {Weight}, {Price}, {Date}

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = [];
}
