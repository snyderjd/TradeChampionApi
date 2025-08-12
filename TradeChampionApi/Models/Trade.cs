using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TradeChampionApi.Interfaces;

namespace TradeChampionApi.Models;

public class Trade : IHasTimestamps
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Ticker { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public required int Quantity { get; set; }

    [Required]
    public required int BuyOrderId { get; set; }

    public Order? BuyOrder { get; set; }
    
    [Required]
    public required int SellOrderId { get; set; }

    public Order? SellOrder { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}