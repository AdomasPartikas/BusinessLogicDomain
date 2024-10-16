using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;

public class PriceHistory
{
    public required int ID { get; set; }
    [Required]
    public decimal EODPrice { get; set; }
    [Required]
    public DateTime Date { get; set; }
}