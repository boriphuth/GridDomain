﻿using System;
using System.Threading;
using GridDomain.Common;
using GridDomain.CQRS.Messaging;
using GridDomain.Node.Configuration.Composition;
using GridDomain.Tests.Unit.CommandsExecution;
using GridDomain.Tests.Unit.Sagas.SagaRecycling.Saga;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace GridDomain.Tests.Unit.Sagas.SagaRecycling
{
    [TestFixture]
    public class Given_State_Saga : InMemorySampleDomainTests
    {
        private Guid _sagaId;
        private State _sagaState;

        [OneTimeSetUp]
        public void When_saga_starts_itself_again()
        {
            var publisher = GridNode.Container.Resolve<IPublisher>();
            _sagaId = Guid.NewGuid();

            publisher.Publish(new StartEvent(Guid.NewGuid()).CloneWithSaga(_sagaId));
            publisher.Publish(new FinishedEvent(Guid.NewGuid()).CloneWithSaga(_sagaId));
            publisher.Publish(new StartEvent(Guid.NewGuid()).CloneWithSaga(_sagaId));

            Thread.Sleep(TimeSpan.FromMilliseconds(1000));

            _sagaState = LoadSagaState<SagaForRecycling, State>(_sagaId);
        }

        [Then]
        public void Saga_state_has_id_from_message()
        {
            Assert.AreEqual(_sagaId, _sagaState.Id);
        }

        [Then]
        public void Saga_state_changed_to_start()
        {
            Assert.AreEqual(States.Created, _sagaState.MachineState);
        }

        protected override IContainerConfiguration CreateConfiguration()
        {
            return new CustomContainerConfiguration(container =>
            {
                container.RegisterStateSaga<SagaForRecycling, State, SagaForRecyclingFactory, StartEvent>(SagaForRecycling.Descriptor);
                container.Register(base.CreateConfiguration());
            });
        }

        protected override IMessageRouteMap CreateMap()
        {
            return new SagaForRecyclingRouteMap();
        }
    }
}
