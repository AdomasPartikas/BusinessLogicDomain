using Hangfire;

namespace BusinessLogicDomain.API.Services
{
    [DisableConcurrentExecution(timeoutInSeconds: 0)]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public interface IMarketDataService
    {
        Task RetrieveAndSaveMarketData();
        Task RetrieveAndSaveAvailableCompanies();
        Task RefreshMarketData();
    }
}