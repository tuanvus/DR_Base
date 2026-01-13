using System;
using System.Collections.Generic;
using System.Threading;

namespace DR_Hive
{
    public class PoolFiber : IFiber, ISubscriptionRegistry, IExecutionContext, IScheduler, IDisposable
    {
        private readonly object _lock = new object();

        // Double buffering queues - swap để tránh lock khi execute
        private List<Action> _queue = new List<Action>();
        private List<Action> _toExecute = new List<Action>();

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _flushPending = false;
        private bool _disposed = false;
        private bool _started = false;

        public void Start()
        {
            lock (_lock)
            {
                if (_started)
                    throw new InvalidOperationException("Fiber already started");
                _started = true;
            }

            // Trigger flush để kick-start fiber
            Enqueue(() => { });
        }

        public void Enqueue(Action action)
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (!_started)
                    throw new InvalidOperationException("Fiber must be started before enqueueing");

                _queue.Add(action);

                if (!_flushPending)
                {
                    ThreadPool.QueueUserWorkItem(Flush);
                    _flushPending = true;
                }
            }
        }

        public void RegisterSubscription(IDisposable toAdd)
        {
            if (toAdd == null) return;

            lock (_lock)
            {
                if (!_disposed)
                {
                    _subscriptions.Add(toAdd);
                }
                else
                {
                    toAdd.Dispose();
                }
            }
        }

        public bool DeregisterSubscription(IDisposable toRemove)
        {
            if (toRemove == null) return false;

            lock (_lock)
            {
                return _subscriptions.Remove(toRemove);
            }
        }

        public IDisposable Schedule(Action action, long firstInMs)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (_disposed) throw new ObjectDisposedException(nameof(PoolFiber));

            var timer = new Timer(_ => Enqueue(action), null, firstInMs, Timeout.Infinite);
            var disposable = new TimerDisposable(timer);
            RegisterSubscription(disposable);
            return disposable;
        }

        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (_disposed) throw new ObjectDisposedException(nameof(PoolFiber));

            var timer = new Timer(_ => Enqueue(action), null, firstInMs, regularInMs);
            var disposable = new TimerDisposable(timer);
            RegisterSubscription(disposable);
            return disposable;
        }

        public void Dispose()
        {
            List<IDisposable> subscriptionsToDispose;

            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;

                subscriptionsToDispose = new List<IDisposable>(_subscriptions);
                _subscriptions.Clear();
                _queue.Clear();
                _toExecute.Clear();
            }

            foreach (var subscription in subscriptionsToDispose)
            {
                try
                {
                    subscription.Dispose();
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            }
        }

        private void Flush(object state)
        {
            // Swap queue trong lock - rất nhanh, chỉ swap reference
            List<Action> actionsToExecute = GetActionsToExecute();

            if (actionsToExecute == null || actionsToExecute.Count == 0)
                return;

            // Execute hoàn toàn BÊN NGOÀI lock - không block enqueue
            foreach (var action in actionsToExecute)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    OnException(ex);
                }
            }

            // Clear list sau khi execute để reuse
            actionsToExecute.Clear();

            // Check xem có items mới được enqueue trong lúc execute không
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    // Còn items, schedule flush tiếp
                    ThreadPool.QueueUserWorkItem(Flush);
                }
                else
                {
                    // Hết items, reset flag
                    _flushPending = false;
                }
            }
        }

        private List<Action> GetActionsToExecute()
        {
            lock (_lock)
            {
                if (_disposed || _queue.Count == 0)
                {
                    _flushPending = false;
                    return null;
                }

                // Swap queue - chỉ swap reference, không copy data
                SwapQueue(ref _queue, ref _toExecute);

                // Clear queue để nhận items mới
                _queue.Clear();
            }

            return _toExecute;
        }

        // Double buffering swap - cực kỳ nhanh, chỉ swap reference
        private void SwapQueue(ref List<Action> a, ref List<Action> b)
        {
            List<Action> temp = b;
            b = a;
            a = temp;
        }

        protected virtual void OnException(Exception ex)
        {
            Console.WriteLine($"[PoolFiber] Exception: {ex}");
        }
    }

    internal class TimerDisposable : IDisposable
    {
        private Timer _timer;
        private bool _disposed;

        public TimerDisposable(Timer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            var timer = Interlocked.Exchange(ref _timer, null);
            timer?.Dispose();
        }
    }
}
