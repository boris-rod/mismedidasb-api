using Hangfire;
using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Utils
{
    public interface IBackgroundJobProcessor
    {
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        Task HandleUserStreaksAsync();
    }
}
