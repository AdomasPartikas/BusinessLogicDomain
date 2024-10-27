using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Entities
{

    public class LivePriceDistinct
    {
        [Key]
        public required string ID { get; set; }
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public required DateTime Date { get; set; }
    }
}

