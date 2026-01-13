using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DR_Hive
{
    public interface IScheduler
    {
        IDisposable Schedule(Action action, long firstInMs);
        IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs);
    }
    public interface ISubscriptionRegistry
    {
        void RegisterSubscription(IDisposable toAdd);

        bool DeregisterSubscription(IDisposable toRemove);
    }

    public interface IExecutionContext
    {
        void Enqueue(Action action);
    }

    public interface IFiber : ISubscriptionRegistry, IExecutionContext, IScheduler, IDisposable
    {
        void Start();
    }
}
