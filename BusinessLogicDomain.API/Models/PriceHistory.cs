namespace BusinessLogicDomain.API.Models;

public class PriceHistory
{
    public required int ID { get; set; }
    public decimal EODPrice { get; set; }
    public DateTime Date { get; set; }
}