/*
 * RECOMMENDED USAGE EXAMPLE
 * 
 * This shows how to use the new pooled DRPeer + DRFiber in your DarkRift server,
 * modeled after Photon patterns (per-room serial Fiber + pooled Peer).
 * 
 * Put your actual game rooms in a separate project (e.g. DR_Hive or your GameLogic).
 */

using DarkRift.Server;
using DR.Common.Concurrency;
using DR.Common.Networking;
using System;
using System.Collections.Concurrent;

namespace DR.Common.Examples
{
    // Example: A simple room that owns its own serial fiber (like Photon Room + Fiber)
    public class ExampleGameRoom : IDisposable
    {
        public string RoomId { get; }
        public DRFiber Fiber { get; private set; }

        private readonly ConcurrentDictionary<ushort, DRPeer> _players = new ConcurrentDictionary<ushort, DRPeer>();

        public ExampleGameRoom(string roomId)
        {
            RoomId = roomId;

            // Rent a fiber from the pool (efficient reuse)
            Fiber = DRFiberPool.Instance.Rent();
            Fiber.Start();
        }

        public void AddPlayer(DRPeer peer)
        {
            // All room mutations MUST go through the fiber for thread safety
            Fiber.Enqueue(() =>
            {
                peer.RoomId = RoomId;
                _players.TryAdd(peer.Id, peer);

                // TODO: send join event, load state, etc.
                Console.WriteLine($"[Room {RoomId}] Player {peer.Id} joined. Total: {_players.Count}");
            });
        }

        public void RemovePlayer(ushort clientId)
        {
            Fiber.Enqueue(() =>
            {
                if (_players.TryRemove(clientId, out DRPeer peer))
                {
                    peer.RoomId = null;
                    // TODO: broadcast leave, check if room empty -> destroy
                    Console.WriteLine($"[Room {RoomId}] Player {clientId} left.");
                }
            });
        }

        public void HandlePlayerMessage(DRPeer peer, ushort tag, object data)
        {
            Fiber.Enqueue(() =>
            {
                // All game logic here runs serially. Safe to touch _players, game state, etc.
                switch (tag)
                {
                    case 1001: // Example Move
                        // Process move, update state, broadcast
                        break;
                }
            });
        }

        public void Dispose()
        {
            // Return fiber to pool when room is destroyed
            if (Fiber != null)
            {
                DRFiberPool.Instance.Return(Fiber);
                Fiber = null;
            }

            _players.Clear();
        }
    }

    // Example improved manager (replace or evolve your old ClientPeerManager)
    public class ServerPeerManager
    {
        public static ServerPeerManager Instance { get; } = new ServerPeerManager();

        private readonly ConcurrentDictionary<ushort, DRPeer> _activePeers = new ConcurrentDictionary<ushort, DRPeer>();

        private ServerPeerManager() { }

        public DRPeer OnClientConnected(IClient client)
        {
            var peer = DRPeerPool.Instance.Rent(client);

            // Optional: attach raw message handler if you want (not recommended for game logic)
            // peer.MessageReceived += (p, msg) => { ... route to room ... };

            _activePeers.TryAdd(client.ID, peer);
            return peer;
        }

        public void OnClientDisconnected(ushort clientId)
        {
            if (_activePeers.TryRemove(clientId, out DRPeer peer))
            {
                // Important: tell the room (via its fiber) that this peer left
                // peer.CurrentRoom?.RemovePlayer(...)

                DRPeerPool.Instance.Return(peer);
            }
        }

        public DRPeer GetPeer(ushort id)
        {
            _activePeers.TryGetValue(id, out var peer);
            return peer;
        }
    }
}
