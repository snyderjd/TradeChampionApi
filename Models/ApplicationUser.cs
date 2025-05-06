using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace TradeChampionApi.Models;

public class ApplicationUser
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; } = "";

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; set; } = "";

    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    public string FullName() => $"{FirstName} {LastName}";

}