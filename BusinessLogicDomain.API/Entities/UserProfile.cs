using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessLogicDomain.API.Entities.Enum;

namespace BusinessLogicDomain.API.Entities
{

    public class UserProfile
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [Required]
        public required virtual User User { get; set; }
        [Required]
        public required decimal Balance { get; set; }
        [Required]
        public required SimulationLevel SimulationLevel { get; set; }
        [Required]
        public required virtual ICollection<UserTransaction> UserTransactions { get; set; } = [];
        [Required]
        public required virtual ICollection<PortfolioStock> UserPortfolioStocks { get; set; } = [];
    }
}