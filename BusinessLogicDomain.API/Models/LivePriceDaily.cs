using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;

public class LivePriceDaily
{
    [Key]
    public required int ID { get; set; }
    [Required]
    public required Company Company { get; set; }
    [Required]
    public required decimal Price { get; set; }
    [Required]
    public required DateTime Date { get; set; }
}