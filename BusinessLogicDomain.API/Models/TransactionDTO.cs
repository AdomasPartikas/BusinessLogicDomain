using BusinessLogicDomain.API.Entities.Enum;

namespace BusinessLogicDomain.API.Models
{
    public class BuyStockDto
    {
        public required string Symbol { get; set; }
        public required decimal Value { get; set; }
        public decimal DeviatedPrice { get; set; }
    }

    public class SellStockDto
    {
        public required string Symbol { get; set; }
        public required decimal Value { get; set; }
        public decimal DeviatedPrice { get; set; }
    }

    public class CancelTransactionDto
    {
        public required int TransactionID { get; set; }
    }

    public class TransactionDto
    {
        public required TransactionType TransactionType { get; set; }
        public required TransactionStatus TransactionStatus { get; set; }
        public required string CompanySymbol { get; set; }
        public required decimal TransactionValue { get; set; }
        public required decimal StockValue { get; set; }
        public required decimal Quantity { get; set; }
        public required DateTime TimeOfTransaction { get; set; }
    }
}