using DarkRift.Server;
using DR.Common.Pooling;
using System;

namespace DR.Common.Networking
{
    /// <summary>
    /// Thread-safe pool for DRPeer instances.
    /// Prevents excessive allocation on high-frequency connect/disconnect (common in lobby/matchmaking scenarios).
    /// 
    /// Usage:
    ///   var peer = DRPeerPool.Instance.Rent(client);
    ///   ...
    ///   DRPeerPool.Instance.Return(peer);
    /// </summary>
    public class DRPeerPool
    {
        public static DRPeerPool Instance { get; } = new DRPeerPool();

        private readonly ObjectPool<DRPeer> _pool;

        private DRPeerPool()
        {
            _pool = new ObjectPool<DRPeer>(
                factory: () => new DRPeer(),
                reset: peer => peer.Reset(),
                maxSize: 1024 // tune based on your CCU churn
            );
        }

        /// <summary>
        /// Get a peer from pool and initialize it for the given client.
        /// </summary>
        public DRPeer Rent(IClient client)
        {
            var peer = _pool.Rent();
            peer.Initialize(client);
            return peer;
        }

        /// <summary>
        /// Return peer to pool (call on disconnect after any room cleanup).
        /// </summary>
        public void Return(DRPeer peer)
        {
            if (peer == null) return;
            _pool.Return(peer);
        }

        public int PooledCount => _pool.Count;
    }
}
