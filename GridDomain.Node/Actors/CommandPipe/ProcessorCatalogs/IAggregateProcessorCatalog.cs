using GridDomain.CQRS;

namespace GridDomain.Node.Actors.CommandPipe.ProcessorCatalogs
{
    public interface IAggregateProcessorCatalog
    {
        /// <summary>
        ///Returns null if no processor was found
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Processor GetAggregateProcessor(ICommand command);
    }
}