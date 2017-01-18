using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Routing;
using GridDomain.Common;
using GridDomain.CQRS;
using GridDomain.CQRS.Messaging.MessageRouting;
using GridDomain.EventSourcing;
using GridDomain.EventSourcing.Sagas;
using GridDomain.Node.Actors;
using GridDomain.Node.Actors.CommandPipe;
using GridDomain.Node.Actors.CommandPipe.ProcessorCatalogs;
using GridDomain.Node.AkkaMessaging;
using GridDomain.Node.AkkaMessaging.Routing;
using Microsoft.Practices.Unity;

namespace GridDomain.Node
{
    public class CommandPipeBuilder : IMessagesRouter
    {
        private readonly AggregateProcessorCatalog _aggregatesCatalog = new AggregateProcessorCatalog();
        private readonly SagaProcessorCatalog _sagaCatalog = new SagaProcessorCatalog();
        private readonly CustomHandlersProcessCatalog _handlersCatalog = new CustomHandlersProcessCatalog();
        private readonly ActorSystem _system;
        private IActorRef _sagasProcessActor;
        private IActorRef _handlersProcessActor;
        private IActorRef _commandExecutorActor;
        private readonly IUnityContainer _container;

        public CommandPipeBuilder(ActorSystem system, IUnityContainer container)
        {
            _container = container;
            _system = system;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Reference to pipe actor for command execution</returns>
        public async Task<IActorRef> Init()
        {
            _sagasProcessActor = _system.ActorOf(Props.Create(() => new SagaProcessActor(_sagaCatalog)));
            _handlersProcessActor = _system.ActorOf(Props.Create(() => new HandlersProcessActor(_handlersCatalog,_sagasProcessActor)));
            _commandExecutorActor = _system.ActorOf(Props.Create(() => new CommandExecutionActor(_aggregatesCatalog)));

            _container.RegisterInstance(HandlersProcessActor.CustomHandlersProcessActorRegistrationName, _handlersProcessActor);

            await _sagasProcessActor.Ask<Initialized>(new Initialize(_commandExecutorActor));
            return _commandExecutorActor;
        }

        public Task RegisterAggregate(IAggregateCommandsHandlerDescriptor descriptor)
        {
            var aggregateHubType = typeof(AggregateHubActor<>).MakeGenericType(descriptor.AggregateType);

            var aggregateActor = CreateActor(aggregateHubType,
                                             NoRouter.Instance,
                                             aggregateHubType.BeautyName());

            var processor = new Processor(aggregateActor);

            foreach (var aggregateCommandInfo in descriptor.RegisteredCommands)
            {
                _aggregatesCatalog.Add(aggregateCommandInfo.CommandType, processor);
            }

            return Task.CompletedTask;
        }

        public Task RegisterSaga(ISagaDescriptor sagaDescriptor, string name = null)
        {
            var sagaActorType = typeof(SagaHubActor<,>).MakeGenericType(sagaDescriptor.SagaType,sagaDescriptor.StateType);
            var sagaActor = CreateActor(sagaActorType, NoRouter.Instance, name ?? sagaDescriptor.StateMachineType.BeautyName());
            var processor = new Processor(sagaActor);

            foreach (var acceptMsg in sagaDescriptor.AcceptMessages)
            {
                _sagaCatalog.Add(acceptMsg.MessageType, processor);
            }

            return Task.CompletedTask;
        }

        public Task RegisterHandler<TMessage, THandler>(string correlationField) where TMessage : DomainEvent where THandler : IHandler<TMessage>
        {
            return RegisterHandler<TMessage, THandler>(true);
        }

        public Task RegisterHandler<TMessage, THandler>(bool sync = false) where THandler : IHandler<TMessage> 
                                                                           where TMessage : DomainEvent
        {
            var handlerActorType = typeof(MessageHandlingActor<TMessage, THandler>);
            var handlerActor = CreateActor(handlerActorType, NoRouter.Instance, handlerActorType.BeautyName());

            _handlersCatalog.Add<TMessage>(new Processor(handlerActor,new MessageProcessPolicy(sync)));
            return Task.CompletedTask;
        }

        private IActorRef CreateActor(Type actorType, RouterConfig routeConfig, string actorName)
        {
            var handleActorProps =_system.DI().Props(actorType);
            handleActorProps = handleActorProps.WithRouter(routeConfig);

            var handleActor = _system.ActorOf(handleActorProps, actorName);
            return handleActor;
        }
    }
}