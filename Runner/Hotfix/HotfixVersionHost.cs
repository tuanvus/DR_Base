using System;
using System.Collections.Generic;
using System.Text;

namespace Hotfix
{
    public sealed class HotfixVersionHost : IDisposable
    {
        public string Version { get; }
        public int ActiveRooms => _activeRooms;

        private int _activeRooms;
        private PluginLoadContext? _alc;
        private WeakReference? _alcWeakRef;

        private IHotfixModule? _module;

        public HotfixVersionHost(string version) => Version = version;

        public void Start(string hotfixDllPath, string moduleTypeFullName)
        {
            _alc = new PluginLoadContext(hotfixDllPath);

            var asm = _alc.LoadFromAssemblyPath(hotfixDllPath);
            var t = asm.GetType(moduleTypeFullName, throwOnError: true)!;

            _module = (IHotfixModule)Activator.CreateInstance(t)!;
            _module.Init();

            _alcWeakRef = new WeakReference(_alc, trackResurrection: true);
        }

        public void RoomCreated(string roomId)
        {
            Interlocked.Increment(ref _activeRooms);
            _module!.OnRoomCreated(roomId);
        }

        public void RoomDisposed(string roomId)
        {
            _module!.OnRoomDisposed(roomId);
            Interlocked.Decrement(ref _activeRooms);
        }

        public void Dispatch(ushort clientId, string roomId, ushort tag, byte[] payload)
            => _module!.OnMessage(clientId, roomId, tag, payload);

        public void Dispose()
        {
            try { _module?.Shutdown(); } catch { }
            _module = null;

            if (_alc != null)
            {
                _alc.Unload(); // bắt đầu tiến trình unload [web:109]
                _alc = null;

                // ép GC để unload “thật sự” (unload là async, cần GC) [web:109][web:174]
                for (int i = 0; _alcWeakRef != null && _alcWeakRef.IsAlive && i < 10; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }
}
