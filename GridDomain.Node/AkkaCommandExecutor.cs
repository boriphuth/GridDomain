using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Util.Internal;
using GridDomain.Common;
using GridDomain.CQRS;
using GridDomain.CQRS.Messaging.Akka;
using GridDomain.Logging;
using GridDomain.Node.Actors;
using GridDomain.Node.AkkaMessaging.Waiting;

namespace GridDomain.Node
{
    /// <summary>
    /// Executes commands. Should not be used inside actors
    /// </summary>
    public class AkkaCommandExecutor : ICommandExecutor
    {
        private readonly IActorTransport _transport;
        private readonly ActorSystem _system;

        public AkkaCommandExecutor(ActorSystem system, IActorTransport transport)
        {
            _system = system;
            _transport = transport;
        }

        public void Execute(params ICommand[] commands)
        {
            foreach (var cmd in commands)
            {
                var metadata = MessageMetadata.Empty()
                                              .CreateChild(cmd.Id, new ProcessEntry(nameof(AkkaCommandExecutor),
                                                                                    "publishing command to transport",
                                                                                    "command is executing"));
                Execute(cmd, metadata);
            }
        }

        public async Task<object> Execute(CommandPlan plan)
        {
            var waiter = new AkkaCommandLocalWaiter(this,_system,_transport,plan.Timeout,true);
            
            var expectBuilder = waiter.ExpectBuilder;

            //All expected messages should be received 
            foreach (var expectedMessage in plan.ExpectedMessages.Where(e => !typeof(IFault).IsAssignableFrom(e.MessageType)))
            {
                expectBuilder.And(MessageMetadataEnvelop.GenericForType(expectedMessage.MessageType), 
                                  o => expectedMessage.Match((o as IMessageMetadataEnvelop)?.Message));
            }


            //All expected faults should end waiting
            foreach (var expectedMessage in plan.ExpectedMessages.Where(e => typeof(IFault).IsAssignableFrom(e.MessageType)))
            {
                expectBuilder.Or(MessageMetadataEnvelop.GenericForType(expectedMessage.MessageType),
                                 o => expectedMessage.Match((o as IMessageMetadataEnvelop)?.Message) &&
                                      (!expectedMessage.Sources.Any() ||
                                        expectedMessage.Sources.Contains(((o as IMessageMetadataEnvelop)?.Message as IFault)?.Processor)));
            }

            //Command fault should always end waiting
            var commandFaultType = typeof(IFault<>).MakeGenericType(plan.Command.GetType());
            expectBuilder.Or(MessageMetadataEnvelop.GenericForType(commandFaultType),
                             o => (((o as IMessageMetadataEnvelop)?.Message as IFault)?.Message as ICommand)?.Id == plan.Command.Id);


            var res = await expectBuilder.Create(plan.Timeout)
                                         .Execute(plan.Command)
                                         .ConfigureAwait(false);

            return res.All.Count > 1 ? res.All.ToArray() : res.All.FirstOrDefault();
        }

        public async Task<T> Execute<T>(CommandPlan<T> plan)
        {
            var res = await Execute((CommandPlan)plan).ConfigureAwait(false);
            var envelop = res as IMessageMetadataEnvelop;
            if (envelop != null && !(typeof(IMessageMetadataEnvelop).IsAssignableFrom(typeof(T))))
                return (T) envelop.Message;

            return (T) res;
        }

        public void Execute<T>(T command, IMessageMetadata metadata) where T : ICommand
        {
               _transport.Publish(command, metadata);
        }
    }
}