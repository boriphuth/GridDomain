﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.Unity;
using GridDomain.CQRS;
using GridDomain.CQRS.Messaging;
using GridDomain.Logging;
using GridDomain.Node.Actors;
using GridDomain.Node.AkkaMessaging.Routing;
using GridDomain.Node.Configuration.Composition;
using GridDomain.Node.Configuration.Persistence;
using GridDomain.Scheduling.Akka.Messages;
using GridDomain.Scheduling.Integration;
using Microsoft.Practices.Unity;

namespace GridDomain.Node
{
    public class GridDomainNode : IGridDomainNode
    {
        private static readonly IDictionary<TransportMode, Type> RoutingActorType = new Dictionary
            <TransportMode, Type>
        {
            {TransportMode.Standalone, typeof (LocalSystemRoutingActor)},
            {TransportMode.Cluster, typeof (ClusterSystemRouterActor)}
        };

        private readonly ISoloLogger _log = LogManager.GetLogger();
        private readonly IMessageRouteMap _messageRouting;
        private readonly TransportMode _transportMode;
        public readonly ActorSystem[] AllSystems;
        public IActorRef PersistentScheduler;
        private Quartz.IScheduler _persistentScheduler;
        public readonly ActorSystem System;
        private IActorRef _mainNodeActor;
        private readonly IContainerConfiguration _configuration;


        public GridDomainNode(IUnityContainer container,
                              IMessageRouteMap messageRouting,
                              TransportMode transportMode,
                              params ActorSystem[] actorAllSystems)
            : this(new EmptyContainerConfig(),messageRouting,transportMode,actorAllSystems)
        {
            Container = container;
        }

        public GridDomainNode(IContainerConfiguration configuration,
                              IMessageRouteMap messageRouting,
                              TransportMode transportMode,
                              params ActorSystem[] actorAllSystems)
        {
            _configuration = configuration;
            _transportMode = transportMode;
            _messageRouting = new CompositeRouteMap(messageRouting, new SchedulingRouteMap());
            AllSystems = actorAllSystems;
            System = AllSystems.Last();
            Container= new UnityContainer();
        }

        public IUnityContainer Container { get; }

        public Guid Id { get; } = Guid.NewGuid();

        public void Start(IDbConfiguration databaseConfiguration)
        {
            Container.RegisterInstance(_messageRouting);

            foreach (var system in AllSystems)
            {
                system.AddDependencyResolver(new UnityDependencyResolver(Container, system));
                Container.CreateChildContainer().Register(new GridNodeContainerConfiguration(system,
                                                    databaseConfiguration,
                                                    _transportMode));
            }

            Container.Register(new GridNodeContainerConfiguration(System,
                                               databaseConfiguration,
                                               _transportMode));

            PersistentScheduler = System.ActorOf(System.DI().Props<SchedulingActor>());
          
            Container.RegisterInstance(new TypedMessageActor<ScheduleMessage>(PersistentScheduler));
            Container.RegisterInstance(new TypedMessageActor<ScheduleCommand>(PersistentScheduler));
            Container.RegisterInstance(new TypedMessageActor<Unschedule>(PersistentScheduler));

            _configuration.Register(Container);

            _persistentScheduler = Container.Resolve<Quartz.IScheduler>();

            StartMainNodeActor(System);
        }

        public void Stop()
        {
            _persistentScheduler.Shutdown(false);
            System.Terminate();
            System.Dispose();


            _log.Info($"GridDomain node {Id} stopped");
        }

        private void StartMainNodeActor(ActorSystem actorSystem)
        {
            _log.Info($"Launching GridDomain node {Id}");

            var props = actorSystem.DI().Props<GridDomainNodeMainActor>();
            _mainNodeActor = actorSystem.ActorOf(props,nameof(GridDomainNodeMainActor));
            _mainNodeActor.Ask(new GridDomainNodeMainActor.Start
            {
                RoutingActorType = RoutingActorType[_transportMode]
            })
            .Wait(TimeSpan.FromSeconds(2));

            _log.Info($"GridDomain node {Id} started at home '{actorSystem.Settings.Home}'");
        }

        public void Execute(params ICommand[] commands)
        {
            foreach(var cmd in commands)
                 _mainNodeActor.Tell(new GridDomainNodeMainActor.ExecuteCommand(cmd));
        }

        //public ICommandStatus ExecuteTracking(ICommand command)
        //{
        //    throw new NotImplementedException();
        //}
    }
}