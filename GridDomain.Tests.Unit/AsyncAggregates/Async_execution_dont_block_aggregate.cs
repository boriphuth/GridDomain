using System;
using System.Threading.Tasks;
using GridDomain.CQRS;
using GridDomain.Node.AkkaMessaging.Waiting;
using GridDomain.Tests.Unit.CommandsExecution;
using GridDomain.Tests.Unit.SampleDomain;
using GridDomain.Tests.Unit.SampleDomain.Commands;
using GridDomain.Tests.Unit.SampleDomain.Events;
using NUnit.Framework;

namespace GridDomain.Tests.Unit.AsyncAggregates
{
    [TestFixture]
    class Async_execution_dont_block_aggregate : InMemorySampleDomainTests
    {
       
        [Test]
        public async Task When_async_method_is_called_other_commands_can_be_executed_before_async_results()
        {
            var aggregateId = Guid.NewGuid();
            var asyncCommand = new AsyncMethodCommand(43, Guid.NewGuid(),Guid.NewGuid(),TimeSpan.FromSeconds(3));
            var syncCommand = new ChangeSampleAggregateCommand(42, aggregateId);

            var asyncCommandTask = GridNode.PrepareCommand(asyncCommand)
                                           .Expect<SampleAggregateChangedEvent>()
                                           .Execute();

            await GridNode.Execute(CommandPlan.New(syncCommand, 
                                                   TimeSpan.FromSeconds(1), 
                                                   Expect.Message<SampleAggregateChangedEvent>(e =>e.SourceId,
                                                   syncCommand.AggregateId)));

            var sampleAggregate = LoadAggregate<SampleAggregate>(syncCommand.AggregateId);

            Assert.AreEqual(syncCommand.Parameter.ToString(), sampleAggregate.Value);
            var waitResults = await asyncCommandTask;
            Assert.AreEqual(asyncCommand.Parameter.ToString(), waitResults.Message<SampleAggregateChangedEvent>().Value);
        }
    }
}