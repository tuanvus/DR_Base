using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DR.Common.Pooling
{
    /// <summary>
    /// Thread-safe generic object pool with support for reset and max size.
    /// Inspired by Photon's RoomReference / object reuse patterns + DarkRift high-churn client scenarios.
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _reset;
        private readonly int _maxSize;
        private int _count;

        /// <summary>
        /// Creates a new pool.
        /// </summary>
        /// <param name="factory">Function to create new instances when pool is empty.</param>
        /// <param name="reset">Optional action to reset/clear state before returning to pool.</param>
        /// <param name="maxSize">Maximum number of objects to keep in pool (0 = unlimited).</param>
        public ObjectPool(Func<T> factory, Action<T> reset = null, int maxSize = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _reset = reset;
            _maxSize = maxSize;
        }

        /// <summary>
        /// Rent an object from the pool (or create new).
        /// </summary>
        public T Rent()
        {
            if (_pool.TryTake(out T item))
            {
                Interlocked.Decrement(ref _count);
                return item;
            }

            return _factory();
        }

        /// <summary>
        /// Return an object to the pool after resetting it.
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;

            _reset?.Invoke(item);

            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                // Pool full, let GC handle it
                return;
            }

            _pool.Add(item);
            Interlocked.Increment(ref _count);
        }

        /// <summary>
        /// Current approximate count of pooled objects.
        /// </summary>
        public int Count => _pool.Count;
    }
}
