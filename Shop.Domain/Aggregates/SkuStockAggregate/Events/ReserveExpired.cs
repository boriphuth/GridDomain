using System;
using GridDomain.EventSourcing;

namespace Shop.Domain.Aggregates.SkuStockAggregate.Events
{
    public class ReserveExpired : DomainEvent
    {
        public Guid CustomerId { get;}

        public ReserveExpired(Guid sourceId, Guid customerId):base(sourceId)
        {
            CustomerId = customerId;
        }
    }
}