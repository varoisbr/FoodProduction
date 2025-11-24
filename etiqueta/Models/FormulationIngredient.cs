using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraLabelPrinter.Models;

/// <summary>
/// Represents an ingredient within a formulation
/// Links a Product (raw material) to a Formulation with quantity and percentage
/// </summary>
public class FormulationIngredient
{
    /// <summary>
    /// Unique identifier for this ingredient entry
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Formulation this ingredient belongs to
    /// </summary>
    [Required]
    public int FormulationId { get; set; }

    /// <summary>
    /// Navigation property to the parent Formulation
    /// </summary>
    [ForeignKey("FormulationId")]
    public virtual Formulation Formulation { get; set; } = null!;

    /// <summary>
    /// Foreign key to the Product (raw material) used as ingredient
    /// </summary>
    [Required]
    public int ProductId { get; set; }

    /// <summary>
    /// Navigation property to the Product
    /// </summary>
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    /// <summary>
    /// Quantity of this ingredient (in kg or other unit)
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Percentage of this ingredient in the formulation
    /// Example: 60.5 means this ingredient represents 60.5% of the total recipe
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal Percentage { get; set; }
}
