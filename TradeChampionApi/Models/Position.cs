using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TradeChampionApi.Interfaces;

namespace TradeChampionApi.Models;

public class Position : IHasTimestamps
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AccountId { get; set; }

    public Account? Account { get; set; }

    [Required]
    public required string Ticker { get; set; }

    [Required]
    public required int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AveragePrice { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}