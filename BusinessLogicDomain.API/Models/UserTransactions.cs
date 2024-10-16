namespace BusinessLogicDomain.API.Models
{
    public class UserTransactions
    {   
        public required int ID { get; set; }
        public required UserProfile UserProfile { get; set; }
        public required TransactionType TransactionType { get; set; }
        public BuyOrder? BuyOrder { get; set; }
        public SellOrder? SellOrder { get; set; }
    }
}