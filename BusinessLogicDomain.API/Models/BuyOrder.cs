using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLogicDomain.API.Models
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
        public required int CompanyId { get; set; }
        [Required]
        public required Company Company { get; set; } = null!;
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required DateTime TimeOfBuying { get; set; }
        [Required]
        public required decimal Quantity { get; set; }
    }
}