using AutoMapper;
using BusinessLogicDomain.API.Context.YouTradeDbContext;
using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.MarketDataDomainAPIClient;

namespace BusinessLogicDomain.API.Services
{
    public class DbService(YouTradeContext context, IMapper mapper) : IDbService
    {
        public readonly YouTradeContext _context = context;
        public readonly IMapper _mapper = mapper;

        public async Task InitializeCompanies(ICollection<StockSymbolDto> stockSymbols)
        {
            _context.Companies.RemoveRange(_context.Companies);
            await _context.SaveChangesAsync();

            var companies = stockSymbols.Select(symbol => _mapper.Map<Company>(symbol)).ToList();
            await _context.Companies.AddRangeAsync(companies);
            await _context.SaveChangesAsync();
        }

        public async Task SaveLiveDistinctMarketData(ICollection<MarketDataDto> marketData)
        {
            _context.LivePriceDistinct.RemoveRange(_context.LivePriceDistinct);
            await _context.SaveChangesAsync();

            var livePriceDistincts = marketData.Select(data => _mapper.Map<LivePriceDistinct>(data)).ToList();
            await _context.LivePriceDistinct.AddRangeAsync(livePriceDistincts);
            await _context.SaveChangesAsync();
        }
    }
}