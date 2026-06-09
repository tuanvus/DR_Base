using Define.Serialization;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DR.Common.Networking
{
    public class OperationClient
    {
        private readonly DRPeer _peer;
        private readonly ConcurrentDictionary<ushort, TaskCompletionSource<object>> _pendingOperations = new ConcurrentDictionary<ushort, TaskCompletionSource<object>>();

        public OperationClient(DRPeer peer)
        {
            _peer = peer;
            _peer.MessageReceived += OnMessageReceived;
        }

        public async Task<TResponse> SendOperationAsync<TRequest, TResponse>(ushort tag, TRequest request, int timeoutMs = 15000)
        {
            var tcs = new TaskCompletionSource<object>();
            if (!_pendingOperations.TryAdd(tag, tcs))
            {
                throw new InvalidOperationException($"Operation with tag {tag} is already pending.");
            }

            try
            {
                byte[] payload = MessagePackDtoSerializer.Instance.Serialize(request);
                _peer.SendMessagePack(tag, payload);

                using (var cts = new CancellationTokenSource(timeoutMs))
                {
                    cts.Token.Register(() => tcs.TrySetResult(null));
                    object result = await tcs.Task;
                    if (result == null) return default;
                    return (TResponse)result;
                }
            }
            finally
            {
                _pendingOperations.TryRemove(tag, out _);
            }
        }

        private void OnMessageReceived(DRPeer peer, Message message)
        {
            if (!_pendingOperations.TryGetValue(message.Tag, out var tcs)) return;

            try
            {
                using (var reader = message.GetReader())
                {
                    int payloadLength = reader.ReadInt32();
                    byte[] payload = reader.ReadBytes();

                    // TODO: Check if it's ErrorResponse
                    object response = MessagePackDtoSerializer.Instance.Deserialize(typeof(object), payload, 0, payloadLength);
                    tcs.TrySetResult(response);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }
    }
}
