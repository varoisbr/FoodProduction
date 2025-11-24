using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraLabelPrinter.Models;

/// <summary>
/// Represents production costs - tracks how much it cost to produce a batch
/// Used for profit calculation: Profit = Sales - Costs
/// </summary>
public class Cost
{
    /// <summary>
    /// Unique identifier for this cost entry
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Production batch this cost is associated with (optional)
    /// Can be null for general costs not tied to specific production
    /// </summary>
    public int? ProductionId { get; set; }

    /// <summary>
    /// Navigation property to the Production
    /// </summary>
    [ForeignKey("ProductionId")]
    public virtual Production? Production { get; set; }

    /// <summary>
    /// Production batch name (denormalized for easier querying)
    /// Can be entered manually if not linked to a Production record
    /// </summary>
    [MaxLength(200)]
    public string ProductionName { get; set; } = string.Empty;

    /// <summary>
    /// Total cost for this production batch or cost entry
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Date when this cost was incurred or entered
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Optional notes about this cost entry
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Calculated cost per kilogram (if linked to Production with weight data)
    /// This is calculated on-the-fly, not stored in database
    /// </summary>
    [NotMapped]
    public decimal CostPerKg
    {
        get
        {
            if (Production != null && Production.Weight > 0)
            {
                return TotalCost / Production.Weight;
            }
            return 0;
        }
    }
}
