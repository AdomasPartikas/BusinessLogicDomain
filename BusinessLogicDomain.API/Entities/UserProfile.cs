using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLogicDomain.API.Entities
{

    public class UserProfile
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public required User User { get; set; }
        [Required]
        public required decimal Balance { get; set; }
        [Required]
        public required ICollection<UserTransactions> UserTransactions { get; set; } = [];
        [Required]
        public required ICollection<SellOrder> SellOrders { get; set; } = [];
        [Required]
        public required ICollection<BuyOrder> BuyOrders { get; set; } = [];
    }
}