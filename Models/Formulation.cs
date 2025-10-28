using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraLabelPrinter.Models;

/// <summary>
/// Represents a recipe/formulation for producing a product
/// Example: A sausage formulation with specific ingredient percentages
/// </summary>
public class Formulation
{
    /// <summary>
    /// Unique identifier for the formulation
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Formulation name (e.g., "Linguica Toscana Recipe V1")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Yield percentage - how much finished product you get from raw materials
    /// Example: 95.5 means 95.5% yield (some weight lost during processing)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal YieldPercentage { get; set; } = 100.00m;

    /// <summary>
    /// Date when this formulation was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Navigation property: Ingredients that make up this formulation
    /// </summary>
    public virtual ICollection<FormulationIngredient> Ingredients { get; set; } = new List<FormulationIngredient>();
}
