using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models
{
    public class UserTransactions
    {   
        [Key]
        public required int ID { get; set; }
        [Required]
        public required UserProfile UserProfile { get; set; }
        [Required]
        public required TransactionType TransactionType { get; set; }
        [Required]
        public BuyOrder? BuyOrder { get; set; }
        [Required]
        public SellOrder? SellOrder { get; set; }
    }
}