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

    public int BuyOrderId { get; set; }
    [Required]
    public required Order BuyOrder { get; set; }
    
    public int SellOrderId { get; set; }
    [Required]
    public required Order SellOrder { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}