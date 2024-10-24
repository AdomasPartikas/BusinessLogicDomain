using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLogicDomain.API.Models;

public class UserProfile
{
    [Key]
    public required int ID { get; set; }
    [Required]
    public required User? User { get; set; }
    [Required]
    public required string Balance { get; set; }
    [Required]
    public required ICollection<UserTransactions> UserTransactions { get; set; } = [];
    [Required]
    public required ICollection<SellOrder> SellOrders { get; set; } = [];
    [Required]
    public required ICollection<BuyOrder> BuyOrders { get; set; } = [];
}