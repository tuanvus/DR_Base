namespace Hotfix
{
    public interface IHotfixModule
    {
        string Version { get; }

        void Init();
        void Shutdown();

        void OnRoomCreated(string roomId);
        void OnRoomDisposed(string roomId);

        void OnMessage(ushort clientId, string roomId, ushort tag, byte[] payload);
    }


}
