using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraLabelPrinter.Models;

/// <summary>
/// Represents a product in the system - can be either a finished product or raw material
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Product name (e.g., "Linguica Toscana", "Sal", "Pimenta")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product type: "Finished" for final products or "Raw" for raw materials/ingredients
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "Finished"; // "Finished" or "Raw"

    /// <summary>
    /// Default price per kilogram for this product
    /// Used as default when creating production entries
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultPricePerKg { get; set; }

    /// <summary>
    /// Navigation property: Productions that used this product
    /// </summary>
    public virtual ICollection<Production> Productions { get; set; } = new List<Production>();

    /// <summary>
    /// Navigation property: Formulation ingredients that reference this product
    /// </summary>
    public virtual ICollection<FormulationIngredient> FormulationIngredients { get; set; } = new List<FormulationIngredient>();
}
