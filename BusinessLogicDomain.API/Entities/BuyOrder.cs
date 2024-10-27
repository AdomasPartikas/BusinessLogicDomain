using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLogicDomain.API.Entities.Enum;

namespace BusinessLogicDomain.API.Entities
{
    public class BuyOrder
    {   
        [Key]
        public required int ID { get; set; }
        [ForeignKey("UserProfileId")]
        public required int UserProfileId { get; set; }
        [Required]
        public required UserProfile UserProfile { get; set; } = null!;
        [ForeignKey("CompanyId")]
        public required string CompanyId { get; set; }
        [Required]
        public required Company Company { get; set; } = null!;
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required DateTime TimeOfBuying { get; set; }
        [Required]
        public required decimal Quantity { get; set; }
        [Required]
        public required TransactionType TransactionType { get; set; }
        [Required]
        public required TransactionStatus TransactionStatus { get; set; }
    }
}