using Hangfire;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IMismeBackgroundService
    {
        Task CleanExpiredTokensAsync();

        Task SendFireBaseNotificationsAsync();

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task SendFireBaseNotificationsRemindersAsync();

        Task RemoveDisabledAccountsAsync();
    }
}
