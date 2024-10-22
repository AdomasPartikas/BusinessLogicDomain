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
    public required ICollection<UserTransactions> UserTransactions { get; set; } = new List<UserTransactions>();
    [Required]
    public required ICollection<SellOrder> SellOrders { get; set; } = new List<SellOrder>();
    [Required]
    public required ICollection<BuyOrder> BuyOrders { get; set; } = new List<BuyOrder>();
}