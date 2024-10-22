using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;

public class TempHourPrice
{
    [Key]
    public required string ID { get; set; }
    [Required]
    public required decimal Price { get; set; }
    [Required]
    public required int TimeStamp { get; set; }
}

