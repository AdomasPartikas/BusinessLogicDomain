namespace BusinessLogicDomain.API.Models
{
    public class BuyOrder
    {   
        public required int ID { get; set; }
        public required UserProfile UserProfile { get; set; }
        public required Company Company { get; set; }
        public required decimal Price { get; set; }
        public required DateTime TimeOfBuying { get; set; }
        public required decimal Quantity { get; set; }
    }
}