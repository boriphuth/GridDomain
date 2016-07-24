using System;
using System.Diagnostics;
using GridDomain.CQRS.Messaging;
using GridDomain.EventSourcing.Sagas.InstanceSagas;
using GridDomain.Node.Configuration.Composition;
using GridDomain.Tests.FutureEvents;
using GridDomain.Tests.Sagas.SoftwareProgrammingDomain.Events;
using GridDomain.Tests.SynchroniousCommandExecute;

namespace GridDomain.Tests.Sagas.InstanceSagas
{
    public class ProgrammingSoftwareSagaTest : SampleDomainCommandExecutionTests
    {
        protected override IMessageRouteMap CreateMap()
        {
            return new SoftwareProgrammingSagaRoutes();
        }

        protected override TimeSpan Timeout => TimeSpan.FromSeconds(Debugger.IsAttached ? 100 : 2);

        protected override IContainerConfiguration CreateConfiguration()
        {
            var baseConf = base.CreateConfiguration();
            return new CustomContainerConfiguration(
                c => c.RegisterSaga<SoftwareProgrammingSaga,
                                    SoftwareProgrammingSagaData,
                                    GotTiredEvent,
                                    SoftwareProgrammingSagaFactory
                    >(),
                c => c.Register(baseConf),
                c => c.RegisterAggregate<SagaDataAggregate<SoftwareProgrammingSagaData>,
                    SagaDataAggregateCommandsHandlerDummy<SoftwareProgrammingSagaData>>()
                );
        }

        public ProgrammingSoftwareSagaTest(bool inMemory = true) : base(inMemory)
        {
        }
    }
}