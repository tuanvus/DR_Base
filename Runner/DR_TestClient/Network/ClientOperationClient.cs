using DarkRift.Client;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;

namespace DR_TestClient.Network
{
    public class ClientOperationClient
    {
        private readonly DarkRiftClient _client;
        private readonly ConcurrentDictionary<ushort, TaskCompletionSource<object>> _pending = new ConcurrentDictionary<ushort, TaskCompletionSource<object>>();
        private readonly ConcurrentDictionary<ushort, Type> _responseTypes = new ConcurrentDictionary<ushort, Type>();

        public ClientOperationClient(DarkRiftClient client)
        {
            _client = client;
            _client.MessageReceived += OnMessageReceived;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(ushort tag, TRequest request, int timeoutMs = 15000)
        {
            var tcs = new TaskCompletionSource<object>();
            if (!_pending.TryAdd(tag, tcs))
                throw new InvalidOperationException($"Operation {tag} already pending.");

            _responseTypes[tag] = typeof(TResponse);

            try
            {
                byte[] payload = MessagePackSerializer.Serialize(request, ContractlessStandardResolver.Options);
                using (var writer = DarkRiftWriter.Create())
                {
                    writer.Write(payload.Length);
                    writer.Write(payload);
                    using (var msg = Message.Create(tag, writer))
                    {
                        _client.SendMessage(msg, SendMode.Reliable);
                    }
                }

                using (var cts = new CancellationTokenSource(timeoutMs))
                {
                    cts.Token.Register(() => tcs.TrySetResult(null));
                    object result = await tcs.Task;
                    return result == null ? default : (TResponse)result;
                }
            }
            finally
            {
                _pending.TryRemove(tag, out _);
                _responseTypes.TryRemove(tag, out _);
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var msg = e.GetMessage())
            {
                if (!_pending.TryGetValue(msg.Tag, out var tcs)) return;
                if (!_responseTypes.TryGetValue(msg.Tag, out var responseType)) return;

                using (var reader = msg.GetReader())
                {
                    int len = reader.ReadInt32();
                    byte[] payload = reader.ReadBytes();
                    object response = MessagePackSerializer.Deserialize(responseType, payload.AsSpan(0, len).ToArray(), ContractlessStandardResolver.Options);
                    tcs.TrySetResult(response);
                }
            }
        }
    }
}
