using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomain;
using CommonDomain.Core;
using GridDomain.EventSourcing;

namespace GridDomain.CQRS.Messaging.MessageRouting
{
    public class AggregateCommandHandler<TAggregate> where TAggregate : AggregateBase
    {
        private readonly Func<ICommand, Guid> _idLocator;
        private readonly Func<ICommand, TAggregate, IReadOnlyCollection<DomainEvent>> _executor;

        private AggregateCommandHandler(Func<ICommand, Guid> idLocator, Func<ICommand,TAggregate, IReadOnlyCollection<DomainEvent>> executor)
        {
            _executor = executor;
            _idLocator = idLocator;
        }

        private static IReadOnlyCollection<DomainEvent> GetAggregateEvents(IAggregate agr)
        {
            return agr.GetUncommittedEvents().Cast<DomainEvent>().ToList();
        } 

        public static AggregateCommandHandler<TAggregate> New<TCommand>(Func<TCommand, Guid> idLocator,
            Action<TCommand, TAggregate> commandExecutor) where TCommand : ICommand
        {
            return new AggregateCommandHandler<TAggregate>(c => idLocator((TCommand)c), 
                (cmd, agr) =>
                {
                    commandExecutor((TCommand)cmd, agr);
                    return GetAggregateEvents(agr);
                });
        }

        public static AggregateCommandHandler<TAggregate> New<TCommand>(Func<TCommand, Guid> idLocator,
            Func<TCommand, TAggregate> commandExecutor)
        {
            return new AggregateCommandHandler<TAggregate>(c => idLocator((TCommand)c),
                (cmd, agr) =>
                {
                    var newAgr = commandExecutor((TCommand) cmd);
                    return GetAggregateEvents(newAgr);
                });
        }

        public Guid GetId(ICommand command)
        {
            return _idLocator(command);
        }

        public IReadOnlyCollection<DomainEvent> Execute(TAggregate agr, ICommand command)
        {
            return _executor(command, agr);
        }
    }
}