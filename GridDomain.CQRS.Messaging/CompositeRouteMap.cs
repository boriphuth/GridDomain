using System.Threading.Tasks;
using GridDomain.CQRS.Messaging.MessageRouting;

namespace GridDomain.CQRS.Messaging
{
    public class CompositeRouteMap : IMessageRouteMap
    {
        private readonly IMessageRouteMap[] _maps;

        public CompositeRouteMap(params IMessageRouteMap[] maps)
        {
            _maps = maps;
        }

        public async Task Register(IMessagesRouter router)
        {
            foreach (var messageRouteMap in _maps)
            {
               await messageRouteMap.Register(router);
            }
        }
    }
}