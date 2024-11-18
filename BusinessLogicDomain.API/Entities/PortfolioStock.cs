using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BusinessLogicDomain.API.Entities
{
 public class PortfolioStock
    {
        [Key]
        public int ID { get; set; }
        
        [ForeignKey("UserProfileId")]
        [JsonIgnore]
        public virtual UserProfile UserProfile { get; set; } = null!;

        [Required]
        public required virtual Company Company { get; set; }

        [Required]
        public required decimal Quantity { get; set; }

        [Required]
        public required decimal CurrentTotalValue { get; set; }

        [Required]
        public required decimal TotalBaseValue { get; set; }

        [Required]
        public required decimal PercentageChange { get; set; }

        [Required]
        public required DateTime LastUpdated { get; set; }
    }
}