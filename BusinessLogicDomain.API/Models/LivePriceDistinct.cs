using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;

public class LivePriceDistinct
{
    [Key]
    public required string ID { get; set; }
    [Required]
    public required decimal Price { get; set; }
    [Required]
    public required DateTime Date { get; set; }
}

