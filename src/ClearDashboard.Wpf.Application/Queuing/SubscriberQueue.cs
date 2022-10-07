using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ClearDashboard.Wpf.Application.Queuing
{
    public interface IProcess
    {

    }

    public abstract class BaseProcess : IProcess
    {
        protected BaseProcess()
        {
            
        }
    }

    public class AddHebrewMaculaProcess : BaseProcess
    {

    }

    public class AddHGreekMaculaProcess : BaseProcess
    {

    }

    public class AddParatextCorpusProcess : BaseProcess
    {

    }

    public class SubscriberQueue
    {
        private readonly BroadcastBlock<IProcess> _processes;

        public SubscriberQueue()
        {
            _processes = new BroadcastBlock<IProcess>(job => job);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : IProcess
        {
            // We have to have a wrapper to work with IJob instead of T
            void ActionWrapper(IProcess process) => handleAction((T)process);

            // create the action block that executes the handler wrapper
            var actionBlock = new ActionBlock<IProcess>(ActionWrapper);

            // Link with Predicate - only if a job is of type T
            _processes.LinkTo(actionBlock, predicate: (job) => job is T);
        }

        public async Task Enqueue(IProcess job)
        {
            await _processes.SendAsync(job);
        }
    }
}
