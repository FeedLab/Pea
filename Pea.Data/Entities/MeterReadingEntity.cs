using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pea.Data.Entities;

/// <summary>
/// Database entity for meter readings - internal to Data project only
/// </summary>
[Table("MeterReadings")]
internal class MeterReadingEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime PeriodStart { get; set; }

    [Required]
    public DateTime PeriodEnd { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal RateA { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal RateB { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal RateC { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Total { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
