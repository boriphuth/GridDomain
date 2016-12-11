using System;
using System.Collections.Generic;

namespace GridDomain.Scheduling.Quartz
{

    public class InMemoryRetrySettings : IRetrySettings
    {
        public int MaxRetries { get; }
        public TimeSpan BackoffBaseInterval { get; }
        public IExceptionPolicy ErrorActions { get; }

        public InMemoryRetrySettings(int maxRetries = 5, TimeSpan? baseInterval = null, IExceptionPolicy errorActions = null)
        {
            ErrorActions = errorActions;
            MaxRetries = maxRetries;
            BackoffBaseInterval = baseInterval ?? TimeSpan.FromMinutes(20);
            ErrorActions = errorActions ?? new AlwaysRetryExceptionPolicy();
        }
    }

    public class AlwaysRetryExceptionPolicy : IExceptionPolicy
    {
        public bool ShouldContinue(Exception ex)
        {
            return true;
        }
    }
}