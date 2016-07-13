using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using GridDomain.CQRS;
using GridDomain.Node.Actors;
using GridDomain.Node.AkkaMessaging.Waiting;

namespace GridDomain.Node
{

    public static class GridNodeExtensions
    {
        public static Task<T> Execute<T>(this IGridDomainNode node, CommandAndConfirmation data)
        {
            return node.Execute(data.Command, data.ExpectedMessages)
                                 .ContinueWith(t =>
                                 {
                                     if (t.IsFaulted)
                                     {
                                         var domainException = t.Exception.UnwrapSingle();
                                         ExceptionDispatchInfo.Capture(domainException).Throw();
                                     }
                                     return (T) t.Result;
                                 });
        }
        public static Task<T> Execute<T>(this IGridDomainNode node, ICommand command, ExpectedMessage expect)
        {
            return Execute<T>(node, new CommandAndConfirmation(command, expect));
        }

        public static T Execute<T>(this IGridDomainNode node, ICommand command, TimeSpan timeout, ExpectedMessage expect)
        {
            return Execute<T>(node, new CommandAndConfirmation(command,expect), timeout);
        }

        public static T Execute<T>(this IGridDomainNode node, CommandAndConfirmation command, TimeSpan timeout)
        {
            var commandExecutionTask = node.Execute<T>(command);
            try
            {
                 if (!commandExecutionTask.Wait(timeout))
                     throw new TimeoutException("Command execution timed out");
                 
                return commandExecutionTask.Result;
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.UnwrapSingle()).Throw();
            }
            
            return commandExecutionTask.Result;
        }

        public static object Execute(this IGridDomainNode node, CommandAndConfirmation command, TimeSpan timeout)
        {
            return Execute<object>(node, command, timeout);
        }


        public static object Execute(this IGridDomainNode node, ICommand command, TimeSpan timeout, params ExpectedMessage[] expect)
        {
            return Execute(node, new CommandAndConfirmation(command, expect), timeout);
        }
    }

}