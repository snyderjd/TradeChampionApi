using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeChampionApi.Interfaces;

namespace TradeChampionApi.Models;

public class Account : IHasTimestamps
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0m;

    public int ApplicationUserId { get; set; }

    [Required]
    public required ApplicationUser ApplicationUser { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}