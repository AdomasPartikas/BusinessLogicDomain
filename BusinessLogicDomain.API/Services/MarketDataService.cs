using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.MarketDataDbContext;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using OfficeOpenXml;

namespace BusinessLogicDomain.API.Services
{
    public class MarketDataService(MarketDataDomainClient marketDataClient, MarketDataContext context) : IMarketDataService
    {
        private readonly MarketDataDomainClient _marketDataClient = marketDataClient;
        private readonly MarketDataContext _context = context;
        public async Task GetMarketData()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var marketData = await _marketDataClient.MarketdataAsync();

            string filePath = "MarketData.xlsx";

            // Create the Excel file if it doesn't exist
            if (!File.Exists(filePath))
            {
                CreateExcelFile(filePath);
            }

            // Append the market data to the Excel file
            AppendMarketDataToExcel(filePath, marketData);

            // Put market data into database
        }

        private void CreateExcelFile(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("MarketData");

            worksheet.Cells[1, 1].Value = "Symbol";
            worksheet.Cells[1, 2].Value = "Description";
            worksheet.Cells[1, 3].Value = "Currency";
            worksheet.Cells[1, 4].Value = "CurrentPrice";
            worksheet.Cells[1, 5].Value = "HighPrice";
            worksheet.Cells[1, 6].Value = "LowPrice";
            worksheet.Cells[1, 7].Value = "OpenPrice";
            worksheet.Cells[1, 8].Value = "PreviousClosePrice";
            worksheet.Cells[1, 9].Value = "Change";
            worksheet.Cells[1, 10].Value = "PercentChange";
            worksheet.Cells[1, 11].Value = "Timestamp";

            package.SaveAs(new FileInfo(filePath));
        }

        private void AppendMarketDataToExcel(string filePath, System.Collections.Generic.ICollection<MarketDataDto> marketData)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets["MarketData"];
            int rowCount = worksheet.Dimension.End.Row;

            foreach (var data in marketData)
            {
                worksheet.Cells[rowCount + 1, 1].Value = data.Symbol;
                worksheet.Cells[rowCount + 1, 2].Value = data.Description;
                worksheet.Cells[rowCount + 1, 3].Value = data.Currency;
                worksheet.Cells[rowCount + 1, 4].Value = data.CurrentPrice;
                worksheet.Cells[rowCount + 1, 5].Value = data.HighPrice;
                worksheet.Cells[rowCount + 1, 6].Value = data.LowPrice;
                worksheet.Cells[rowCount + 1, 7].Value = data.OpenPrice;
                worksheet.Cells[rowCount + 1, 8].Value = data.PreviousClosePrice;
                worksheet.Cells[rowCount + 1, 9].Value = data.Change;
                worksheet.Cells[rowCount + 1, 10].Value = data.PercentChange;
                worksheet.Cells[rowCount + 1, 11].Value = data.Timestamp;

                rowCount++;
            }

            package.Save();
        }
        private async Task CreateMarketDataEntriesInDb(ICollection<MarketDataDto> marketData){
            foreach (var data in marketData){
                // var tempHourPrice = new TempHourPrice{
                    
                // };
                // _context.TempHourPrices.Add(tempHourPrice);
                // _context.SaveChanges();
            }
        }
    }
}