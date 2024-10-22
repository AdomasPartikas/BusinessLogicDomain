using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;

public class TempDayPrice
{
    [Key]
    public required int ID { get; set; }
    [Required]
    public required decimal Price { get; set; }
    [Required]
    public required int TimeStamp { get; set; }
}