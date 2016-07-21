using System;
using GridDomain.CQRS;

namespace GridDomain.EventSourcing.Sagas.FutureEvents
{
    public class RaiseScheduledDomainEventCommand : Command
    {
        public Guid FutureEventId { get; }

        public Guid AggregateId { get; }

        public RaiseScheduledDomainEventCommand(Guid futureEventId, Guid aggregateId)
        {
            FutureEventId = futureEventId;
            AggregateId = aggregateId;
        }
    }
}