using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models
{
    public class Company
    {
        [Key]
        public required int ID { get; set; }
        [Required]
        public required string Name { get; set; }
    }
}