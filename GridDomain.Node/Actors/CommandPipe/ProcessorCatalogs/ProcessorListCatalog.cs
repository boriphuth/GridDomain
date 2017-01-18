using System.Collections.Generic;

namespace GridDomain.Node.Actors.CommandPipe.ProcessorCatalogs
{
    class ProcessorListCatalog<TMessage> : TypeCatalog<List<Processor>,TMessage>
    {
        public override void Add<U>(Processor processor)
        {
            List<Processor> list;
            var messageType = typeof(U);
            if (!Catalog.TryGetValue(messageType, out list))
                list = Catalog[messageType] = new List<Processor>();

            list.Add(processor);
        }
        protected new IReadOnlyCollection<Processor> GetProcessor<U>(U message) where U:TMessage
        {
            return base.GetProcessor(message);
        }
    }
}