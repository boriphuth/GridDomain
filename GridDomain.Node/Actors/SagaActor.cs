﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka;
using Akka.Persistence;
using CommonDomain.Core;
using GridDomain.CQRS;
using GridDomain.CQRS.Messaging;
using GridDomain.EventSourcing;
using GridDomain.EventSourcing.Sagas;

namespace GridDomain.Node.Actors
{
    /// <summary>
    ///     Name should be parse by AggregateActorName
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TSagaState"></typeparam>
    /// <typeparam name="TStartMessage"></typeparam>
    public class SagaActor<TSaga, TSagaState, TStartMessage> :
        ReceivePersistentActor where TSaga : IDomainSaga
        where TSagaState : AggregateBase
        where TStartMessage : DomainEvent
    {
        private readonly IPublisher _publisher;
        private readonly ISagaFactory<TSaga, TStartMessage> _sagaStarter;
        public TSaga Saga;
        private readonly ISagaFactory<TSaga, TSagaState> _sagaFactory;

        public SagaActor(ISagaFactory<TSaga, TStartMessage> sagaStarter,
                         ISagaFactory<TSaga, TSagaState> sagaFactory,
                         IEmptySagaFactory<TSaga> emptySagaFactory,
                         IPublisher publisher)
        {
            _sagaStarter = sagaStarter;
            _sagaFactory = sagaFactory;
            _publisher = publisher;
            Saga = emptySagaFactory.Create(); //need empty saga for recovery from persistence storage


            Command<ICommandFault>(ProcessSaga,fault => fault.SagaId == Saga.State.Id);
            Command<DomainEvent>(ProcessSaga,cmd => cmd.SagaId == Saga.State.Id);
            Command<TStartMessage>(startMessage =>
            {
                Saga = _sagaStarter.Create(startMessage);
                ProcessSaga(startMessage);
            },start => Saga.State.Id == Guid.Empty); //duplicate start event
            Recover<SnapshotOffer>(offer => Saga = _sagaFactory.Create((TSagaState) offer.Snapshot));
            Recover<DomainEvent>(e => Saga.State.ApplyEvent(e));
        }

        private void ProcessSaga(object message)
        {
            Saga.Transit(message);

            var stateChangeEvents = Saga.State.GetUncommittedEvents().Cast<object>();
            PersistAll(stateChangeEvents, e => _publisher.Publish(e));

            foreach (var msg in Saga.CommandsToDispatch)
                _publisher.Publish(msg);

            Saga.ClearCommandsToDispatch();
            Saga.State.ClearUncommittedEvents();
        }

        public override string PersistenceId => Self.Path.Name;
    }
}