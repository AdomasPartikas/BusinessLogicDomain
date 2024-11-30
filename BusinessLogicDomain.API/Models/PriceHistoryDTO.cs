namespace BusinessLogicDomain.API.Models
{
    public class PriceHistoryDTO
    {
        public required string CompanySymbol { get; set; }
        public required decimal EODPrice { get; set; }
        public required DateTime Date { get; set; }
    }
}