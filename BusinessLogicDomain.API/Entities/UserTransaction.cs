using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLogicDomain.API.Entities.Enum;
using Newtonsoft.Json;

namespace BusinessLogicDomain.API.Entities
{
    public class UserTransaction
    {   
        [Key]
        public int ID { get; set; }
        [ForeignKey("UserProfileId")]
        [JsonIgnore]
        public virtual UserProfile UserProfile { get; set; } = null!;
        [Required]
        public required TransactionType TransactionType { get; set; }
        [Required]
        public required TransactionStatus TransactionStatus { get; set; }
        [Required]
        public required virtual Company Company { get; set; }
        [Required]
        public required decimal TransactionValue { get; set; }
        [Required]
        public required decimal StockValue { get; set; }
        [Required]
        public required decimal Quantity { get; set; }
        [Required]
        public required DateTime TimeOfTransaction { get; set; }
    }
}