using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;

public class TempHourPrice
{
    public int ID { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int TimeStamp { get; set; }
}

