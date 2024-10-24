using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLogicDomain.API.Models.Enum;

namespace BusinessLogicDomain.API.Models
{
    public class SellOrder
    {   
        [Key]
        public required int ID { get; set; }
        [ForeignKey("UserProfileId")]
        public required int UserProfilerId { get; set; }
        [Required]
        public required UserProfile UserProfile { get; set; } = null!;
        [ForeignKey("CompanyId")]
        public required int CompanyId { get; set; }
        [Required]
        public required Company Company { get; set; }
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required DateTime TimeOfSelling { get; set; }
        [Required]
        public required decimal Quantity { get; set; }
        [Required]
        public required TransactionType TransactionType { get; set; }
    }
}