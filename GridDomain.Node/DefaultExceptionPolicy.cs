using System;
using GridDomain.Common;
using GridDomain.EventSourcing.Sagas.FutureEvents;
using GridDomain.Scheduling.Quartz;

namespace GridDomain.Node
{
    public class DefaultExceptionPolicy : IExceptionPolicy
    {
        public bool ShouldContinue(Exception ex)
        {
            if (ex.UnwrapSingle() is NullReferenceException)
                return false;

            if (ex.UnwrapSingle() is ScheduledEventNotFoundException)
                return false;

            return true;
        }
    }
}