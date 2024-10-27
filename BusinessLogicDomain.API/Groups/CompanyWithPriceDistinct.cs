using BusinessLogicDomain.API.Entities;

namespace BusinessLogicDomain.API.Groups
{
    public record CompanyWithPriceDistinct
    {
        public LivePriceDistinct LivePriceDistinct { get; set; }
        public Company Company { get; set; }
    }
}