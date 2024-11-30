using AutoMapper;
using BusinessLogicDomain.API.Groups;
using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using BusinessLogicDomain.API.Models;

namespace BusinessLogicDomain.API.Profile
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
                
        public AutoMapperProfile()
        {

            CreateMap<StockSymbolDto, Company>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Symbol))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Description));

            CreateMap<MarketDataDto, LivePriceDistinct>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Symbol))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.CurrentPrice))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date));

            CreateMap<CompanyWithPriceDistinct, LivePriceDaily>()
                .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.LivePriceDistinct.Price))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.LivePriceDistinct.Date));

            CreateMap<LivePriceDaily, PriceHistory>()
                .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
                .ForMember(dest => dest.EODPrice, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date));

            CreateMap<PriceHistory, PriceHistoryDTO>()
                .ForMember(dest => dest.CompanySymbol, opt => opt.MapFrom(src => src.Company.ID));
        }
    }
}