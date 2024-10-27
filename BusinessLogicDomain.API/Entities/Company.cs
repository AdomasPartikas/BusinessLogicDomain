using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Entities
{
    public class Company
    {
        [Key]
        public required string ID { get; set; }
        [Required]
        public required string Name { get; set; }
    }
}