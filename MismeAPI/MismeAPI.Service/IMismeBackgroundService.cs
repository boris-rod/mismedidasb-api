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

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task HandleUserStreaksAsync(int timeOffsetRange);

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task HandleSubscriptionsAsync();

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task SendPlanifyEventNotificationAsync();

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task SendReportsAsync();

        [DisableConcurrentExecution(timeoutInSeconds: 1 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task ProcessStoredEmailsAsync();
    }
}
