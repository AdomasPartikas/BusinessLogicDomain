using System.ComponentModel.DataAnnotations;
using BusinessLogicDomain.API.Entities.Enum;

namespace BusinessLogicDomain.API.Entities
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