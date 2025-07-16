using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeChampionApi.Enums;
using TradeChampionApi.Interfaces;

namespace TradeChampionApi.Models;

public class Order : IHasTimestamps
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required int AccountId { get; set; }

    public Account? Account { get; set; }

    [Required]
    public required string Ticker { get; set; }

    [Required]
    public OrderType OrderType { get; set; }

    [Required]
    public OrderSide Side { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public required int Quantity { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}