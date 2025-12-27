using System;
using System.Collections.Generic;
using System.Text;

namespace Hotfix
{
    public sealed class HotfixManager
    {
        private readonly object _lock = new();

        private readonly Dictionary<string, HotfixVersionHost> _versions = new();
        private readonly Dictionary<string, string> _roomToVersion = new(); // roomId -> version

        public string DefaultVersion { get; private set; } = "1.0";

        public void LoadVersion(string version, string hotfixDllPath, string moduleTypeFullName)
        {
            lock (_lock)
            {
                if (_versions.ContainsKey(version)) return;

                var host = new HotfixVersionHost(version);
                host.Start(hotfixDllPath, moduleTypeFullName);
                _versions[version] = host;
            }
        }

        public void SwitchDefault(string version)
        {
            lock (_lock) DefaultVersion = version;
        }

        public void CreateRoom(string roomId, string? forcedVersion = null)
        {
            lock (_lock)
            {
                var version = forcedVersion ?? DefaultVersion;
                _roomToVersion[roomId] = version;
                _versions[version].RoomCreated(roomId);
            }
        }

        public void DisposeRoom(string roomId)
        {
            lock (_lock)
            {
                if (!_roomToVersion.TryGetValue(roomId, out var version)) return;
                _roomToVersion.Remove(roomId);
                _versions[version].RoomDisposed(roomId);
            }
        }

        public void DispatchToRoom(ushort clientId, string roomId, ushort tag, byte[] payload)
        {
            HotfixVersionHost host;
            lock (_lock)
            {
                var version = _roomToVersion[roomId];      // room đang chạy v1.0 thì mãi v1.0
                host = _versions[version];
            }

            host.Dispatch(clientId, roomId, tag, payload);
        }

        public void TryDrainAndUnload(string version)
        {
            HotfixVersionHost? host;
            lock (_lock)
            {
                if (!_versions.TryGetValue(version, out host)) return;
                if (host.ActiveRooms != 0) return; // còn room thì chưa unload
                _versions.Remove(version);
            }

            host.Dispose(); // unload ALC + GC
        }
    }

}
