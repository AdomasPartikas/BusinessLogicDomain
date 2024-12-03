namespace BusinessLogicDomain.API.Models
{
    public class PriceHistoryDto
    {
        public required string CompanySymbol { get; set; }
        public required decimal EODPrice { get; set; }
        public required DateTime Date { get; set; }
    }
}