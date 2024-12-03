using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Entities.Enum;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Hangfire;

namespace BusinessLogicDomain.API.Services
{
    public class TransactionService(IMarketDataDomainClient marketDataClient, IDbService dbService) : ITransactionService
    {
        private readonly IMarketDataDomainClient _marketDataClient = marketDataClient;
        private readonly IDbService _dbService = dbService;

        /*public async Task<UserTransaction> ExecuteTransaction(UserProfile userProfile, UserTransaction transaction) //Naudojamas Kokybei Lauros
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
        }*/

    public async Task<UserTransaction> ExecuteTransaction(UserProfile userProfile, UserTransaction transaction)
    {
    
        if (!ValidateTransaction(userProfile, transaction))
        {
            SetTransactionCancelled(transaction);
            await _dbService.UpdateTransaction(transaction);
            return transaction;
        }

        if (!await IsMarketOpenAsync())
        {
            UpdateTransactionStatus(transaction, TransactionStatus.OnHold);
            await _dbService.UpdateTransaction(transaction);
            return transaction;
        }

        if (transaction.TransactionStatus == TransactionStatus.OnHold)
        {
            await RecalculateTransaction(transaction);
        }

        await HandleTransactionTypeAsync(userProfile, transaction);

        UpdateTransactionStatus(transaction, TransactionStatus.Completed);
        await _dbService.UpdateTransaction(transaction);

        return transaction;
    }

    private async Task<bool> IsMarketOpenAsync()
    {
        var marketStatus = await _marketDataClient.MarketstatusAsync();

        if (marketStatus == null)
        {
            throw new InvalidOperationException("Market status is null.");
        }

        return marketStatus.IsOpen;
    }
    private void UpdateTransactionStatus(UserTransaction transaction, TransactionStatus status)
    {
        transaction.TransactionStatus = status;
        transaction.TimeOfTransaction = DateTime.Now;
    }
    private void SetTransactionCancelled(UserTransaction transaction)
    {
        UpdateTransactionStatus(transaction, TransactionStatus.Cancelled);
    }    
    private async Task HandleTransactionTypeAsync(UserProfile userProfile, UserTransaction transaction)
    {
        switch (transaction.TransactionType)
        {
            case TransactionType.Buy:
                await ExecuteBuyTransaction(userProfile, transaction);
                break;

            case TransactionType.Sell:
                await ExecuteSellTransaction(userProfile, transaction);
                break;

            default:
                throw new InvalidOperationException($"Unsupported transaction type: {transaction.TransactionType}");
        }
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

        /*
                private bool ValidateTransaction(UserProfile userProfile, UserTransaction transaction) //Naudojamas Kokybei Patriko
                {
                    //Naudosime SRP
                    //Iskaidisime i skitingas mazesnes funkcijas, sumazinsime sio metodo dydi

                    if(userProfile == null) //1 
                        return false;

                    if(transaction == null) //2
                        return false;

                    if(transaction.Company == null) //3
                        return false;

                    if(transaction.Quantity <= 0) //4
                        return false;

                    if(transaction.TransactionValue <= 0) //5
                        return false;

                    if(transaction.StockValue <= 0) //6
                        return false;

                    if(transaction.TransactionType == TransactionType.Buy) //7
                    {
                        if(transaction.TransactionValue > userProfile.Balance) //8
                            return false;
                    }
                    else if(transaction.TransactionType == TransactionType.Sell) //9
                    {
                        if(userProfile.UserPortfolioStocks.Where(x => x.Company.ID == transaction.Company.ID).Any() && //10
                            transaction.Quantity > userProfile.UserPortfolioStocks.Where(x => x.Company.ID == transaction.Company.ID).FirstOrDefault()!.Quantity) //11
                            return false;
                    }

                    return true;
                }
        */

        private static bool ValidateTransaction(UserProfile userProfile, UserTransaction transaction)
        {
            if (IsInvalidUserProfile(userProfile)) //User if
                return false;

            if (IsInvalidTransaction(transaction)) //All transaction ifs
                return false;

            return transaction.TransactionType switch //Switch with buy/sell
            {
                TransactionType.Buy => IsBuyTransactionValid(userProfile, transaction),
                TransactionType.Sell => IsSellTransactionValid(userProfile, transaction),
                _ => false,
            };
        }

        private static bool IsInvalidUserProfile(UserProfile userProfile)
        {
            return userProfile == null;
        }

        private static bool IsInvalidTransaction(UserTransaction transaction)
        {
            return transaction == null ||
                   transaction.Company == null ||
                   transaction.Quantity <= 0 ||
                   transaction.TransactionValue <= 0 ||
                   transaction.StockValue <= 0;
        }

        private static bool IsBuyTransactionValid(UserProfile userProfile, UserTransaction transaction)
        {
            return transaction.TransactionValue <= userProfile.Balance;
        }

        private static bool IsSellTransactionValid(UserProfile userProfile, UserTransaction transaction)
        {
            var portfolioStock = userProfile.UserPortfolioStocks
                .FirstOrDefault(x => x.Company.ID == transaction.Company.ID);

            return portfolioStock != null && transaction.Quantity <= portfolioStock.Quantity;
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

        private async Task RecalculatePortfolio(UserProfile userProfile) //Naudojamas Kokybei
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