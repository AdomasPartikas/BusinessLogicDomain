using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Entities.Enum;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Hangfire;

namespace BusinessLogicDomain.API.Services
{
    public class TransactionService(MarketDataDomainClient marketDataClient, IDbService dbService) : ITransactionService
    {
        private readonly MarketDataDomainClient _marketDataClient = marketDataClient;
        private readonly IDbService _dbService = dbService;

        public async Task<UserTransaction> ExecuteTransaction(UserProfile userProfile, UserTransaction transaction)
        {
            if(!ValidateTransaction(userProfile, transaction))
            {
                transaction.TransactionStatus = TransactionStatus.Cancelled;
                transaction.TimeOfTransaction = DateTime.Now;

                await _dbService.UpdateTransaction(transaction);

                return transaction;
            }

            var marketStatus = await _marketDataClient.MarketstatusAsync();

            if(!marketStatus.IsOpen)
            {
                transaction.TransactionStatus = TransactionStatus.OnHold;
                transaction.TimeOfTransaction = DateTime.Now;

                await _dbService.UpdateTransaction(transaction);

                return transaction;
            }

            if(transaction.TransactionStatus == TransactionStatus.OnHold)
                await RecalculateTransaction(transaction);

            if(transaction.TransactionType == TransactionType.Buy)
                await ExecuteBuyTransaction(userProfile, transaction);
            else if(transaction.TransactionType == TransactionType.Sell)
                await ExecuteSellTransaction(userProfile, transaction);

            transaction.TransactionStatus = TransactionStatus.Completed;

            await _dbService.UpdateTransaction(transaction);

            return transaction;
        }
        private async Task RecalculateTransaction(UserTransaction transaction)
        {
            var company = await _dbService.RetrieveCompanyBySymbol(transaction.Company.ID);

            if (company == null)
            return;

            transaction.StockValue = await _dbService.GetCurrentStockPriceOfCompany(company.ID);
            transaction.Quantity = transaction.TransactionValue / transaction.StockValue;
        }

        private async Task ExecuteSellTransaction(UserProfile userProfile, UserTransaction transaction)
        {
            userProfile.Balance += transaction.TransactionValue;

            var existingPortfolioStock = userProfile.UserPortfolioStocks.FirstOrDefault(ps => ps.Company.ID == transaction.Company.ID);

            if (existingPortfolioStock != null)
            {
                existingPortfolioStock.Quantity -= transaction.Quantity;
                existingPortfolioStock.CurrentTotalValue -= transaction.TransactionValue;
                existingPortfolioStock.TotalBaseValue -= transaction.TransactionValue;
                existingPortfolioStock.PercentageChange = (existingPortfolioStock.CurrentTotalValue - existingPortfolioStock.TotalBaseValue) / existingPortfolioStock.TotalBaseValue;
                existingPortfolioStock.LastUpdated = DateTime.Now;

                if (existingPortfolioStock.Quantity == 0)
                    userProfile.UserPortfolioStocks.Remove(existingPortfolioStock);
            }

            await _dbService.UpdateUserProfile(userProfile);
        }

        private async Task ExecuteBuyTransaction(UserProfile userProfile, UserTransaction transaction)
        {
            userProfile.Balance -= transaction.TransactionValue;

            var existingPortfolioStock = userProfile.UserPortfolioStocks.FirstOrDefault(ps => ps.Company.ID == transaction.Company.ID);

            if (existingPortfolioStock == null)
            {
                existingPortfolioStock = new PortfolioStock
                {
                    Company = transaction.Company,
                    Quantity = transaction.Quantity,
                    CurrentTotalValue = transaction.TransactionValue,
                    TotalBaseValue = transaction.TransactionValue,
                    PercentageChange = 0,
                    LastUpdated = DateTime.Now
                };

                userProfile.UserPortfolioStocks.Add(existingPortfolioStock);
            }
            else
            {
                existingPortfolioStock.Quantity += transaction.Quantity;
                existingPortfolioStock.CurrentTotalValue += transaction.TransactionValue;
                existingPortfolioStock.TotalBaseValue += transaction.TransactionValue;
                existingPortfolioStock.LastUpdated = DateTime.Now;
            }

            await _dbService.UpdateUserProfile(userProfile);
        }

        private bool ValidateTransaction(UserProfile userProfile, UserTransaction transaction)
        {
            if(userProfile == null)
                return false;

            if(transaction == null)
                return false;

            if(transaction.Company == null)
                return false;

            if(transaction.Quantity <= 0)
                return false;

            if(transaction.TransactionValue <= 0)
                return false;

            if(transaction.StockValue <= 0)
                return false;

            if(transaction.TransactionType == TransactionType.Buy)
            {
                if(transaction.TransactionValue > userProfile.Balance)
                    return false;
            }
            else if(transaction.TransactionType == TransactionType.Sell)
            {
                if(userProfile.UserPortfolioStocks.Where(x => x.Company.ID == transaction.Company.ID).Any() &&
                    transaction.Quantity > userProfile.UserPortfolioStocks.Where(x => x.Company.ID == transaction.Company.ID).FirstOrDefault()!.Quantity)
                    return false;
            }

            return true;
        }

        public async Task<UserTransaction> CancelTransaction(UserTransaction transaction)
        {
            if(transaction.TransactionStatus == TransactionStatus.OnHold)
                transaction.TransactionStatus = TransactionStatus.Cancelled;

            await _dbService.UpdateTransaction(transaction);

            return transaction;
        }

        public async Task CreateIndividualJobs()
        {
            //TODO: Execute transactions by order date 
            var userProfiles = await _dbService.RetrieveAllUserProfiles();

            foreach (var userProfile in userProfiles)
            {
                foreach (var transaction in userProfile.UserTransactions)
                {
                    await RefreshUserProfile(userProfile);
                }
            }
        }

        private async Task RefreshUserProfile(UserProfile userProfile)
        {
            await RunOnHoldTransactions(userProfile);

            await RecalculatePortfolio(userProfile);

            await _dbService.UpdateUserProfile(userProfile);
        }

        private async Task RecalculatePortfolio(UserProfile userProfile)
        {
            foreach (var portfolioStock in userProfile.UserPortfolioStocks)
            {
                portfolioStock.CurrentTotalValue = portfolioStock.Quantity * await _dbService.GetCurrentStockPriceOfCompany(portfolioStock.Company.ID);
                portfolioStock.PercentageChange = (portfolioStock.CurrentTotalValue - portfolioStock.TotalBaseValue) / portfolioStock.TotalBaseValue;
                portfolioStock.LastUpdated = DateTime.Now;
            }
        }

        private async Task RunOnHoldTransactions(UserProfile userProfile)
        {
            foreach (var transaction in userProfile.UserTransactions)
            {
                if(transaction.TransactionStatus == TransactionStatus.OnHold)
                    await ExecuteTransaction(userProfile, transaction);
            }
        }
    }
}