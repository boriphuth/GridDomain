using System;
using GridDomain.EventSourcing;
using NMoneys;

namespace GridDomain.Balance.Domain.ServiceSubscriptionAggregate
{
    class ServiceSubscriptionCreatedEvent : DomainEvent
    {
        public TimeSpan Period;
        public Money Cost;
        public SubscriptionRights Rights;

        public ServiceSubscriptionCreatedEvent(Guid sourceId, DateTime? createdTime = null) : base(sourceId, createdTime)
        {
        }
    }
}