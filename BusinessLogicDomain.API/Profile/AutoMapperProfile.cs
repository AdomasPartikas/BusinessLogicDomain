using AutoMapper;
using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.MarketDataDomainAPIClient;

namespace BusinessLogicDomain.API.Profile
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StockSymbolDto, Company>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Description));

            CreateMap<MarketDataDto, LivePriceDistinct>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Symbol))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.CurrentPrice))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date));
        }
    }
}