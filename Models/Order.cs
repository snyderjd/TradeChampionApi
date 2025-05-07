using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeChampionApi.Enums;

namespace TradeChampionApi.Models;

public class Order
{
    [Key]
    public int Id { get; set; }
    public int AccountId { get; set; }
    [Required]
    public required Account Account { get; set; }

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
    public OrderStatus Status { get; set; }
}