using BusinessLogicDomain.API.Entities;
using Hangfire;

namespace BusinessLogicDomain.API.Services
{
    [DisableConcurrentExecution(timeoutInSeconds: 0)]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public interface ITransactionService
    {
        Task<UserTransaction> ExecuteTransaction(UserProfile userProfile, UserTransaction transaction);
        Task CreateIndividualJobs();
    }
}