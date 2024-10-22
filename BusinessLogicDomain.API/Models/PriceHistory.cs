using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLogicDomain.API.Models;

public class PriceHistory
{
    [Key]
    public required int ID { get; set; }
    [ForeignKey("CompanyId")]
    public required int CompanyId { get; set; }
    [Required]
    public required Company Company { get; set; }
    [Required]
    public required decimal EODPrice { get; set; }
    [Required]
    public required DateTime Date { get; set; }
}