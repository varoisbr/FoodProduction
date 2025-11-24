using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraLabelPrinter.Models;

/// <summary>
/// Represents a production entry - each time a label is printed
/// Stores weight, price, and total for each production batch item
/// This replaces the old CSV storage system
/// </summary>
public class Production
{
    /// <summary>
    /// Unique identifier for this production entry
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Production batch name (e.g., "Lote 001", "Producao Manha")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the Product being produced (optional - can be null for legacy entries)
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Navigation property to the Product
    /// </summary>
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    /// <summary>
    /// Weight of this production item in kilograms
    /// </summary>
    [Column(TypeName = "decimal(18,3)")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Price per kilogram at the time of production
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerKg { get; set; }

    /// <summary>
    /// Total price (Weight × PricePerKg)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    /// <summary>
    /// Date and time when this production entry was created
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Path to the ZPL template file used for printing (optional)
    /// Stores the template path for reference
    /// </summary>
    [MaxLength(500)]
    public string? ZplTemplatePath { get; set; }

    /// <summary>
    /// Navigation property: Costs associated with this production
    /// </summary>
    public virtual ICollection<Cost> Costs { get; set; } = new List<Cost>();
}
