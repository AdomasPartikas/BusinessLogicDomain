using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models
{
    public class BuyOrder
    {   
        public required int ID { get; set; }
        [Required]
        public required UserProfile UserProfile { get; set; }
        [Required]
        public required Company Company { get; set; }
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required DateTime TimeOfBuying { get; set; }
        [Required]
        public required decimal Quantity { get; set; }
    }
}