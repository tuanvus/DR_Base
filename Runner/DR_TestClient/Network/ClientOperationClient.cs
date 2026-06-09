using DarkRift.Client;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Concurrent;
using System.Threading;
using DarkRift;

namespace DR_TestClient.Network
{
    /// <summary>
    /// Gửi operation request và nhận response qua **callback duy nhất**.
    /// 
    /// Chỉ dùng 1 callback (Action&lt;TResponse&gt;). 
    /// Không có onError riêng vì response DTO của bạn đã chứa mã code + data bên trong.
    /// 
    /// - Thành công: callback được gọi với data thật.
    /// - Timeout / lỗi: callback vẫn được gọi với default(TResponse) (thường là null).
    ///   Bạn tự kiểm tra mã code / null / error data bên trong callback.
    /// 
    /// Lưu ý: hiện đang key theo tag (opcode) nên chỉ 1 request pending cho cùng tag tại 1 thời điểm.
    /// </summary>
    public class ClientOperationClient
    {
        private readonly DarkRiftClient _client;
        private readonly ConcurrentDictionary<ushort, IPending> _pendings = new ConcurrentDictionary<ushort, IPending>();

        public ClientOperationClient(DarkRiftClient client)
        {
            _client = client;
            _client.MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Bắn request, nhận kết quả qua callback (chỉ 1 callback).
        /// Callback sẽ luôn được gọi đúng 1 lần.
        /// </summary>
        public void SendAsync<TRequest, TResponse>(
            ushort tag,
            TRequest request,
            Action<TResponse> callback,
            int timeoutMs = 15000)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var pending = new Pending<TResponse> { Callback = callback };

            // Cùng tag thì thay thế callback cũ (last wins - tiện cho test)
            if (!_pendings.TryAdd(tag, pending))
            {
                if (_pendings.TryRemove(tag, out var old))
                    old.CancelTimeout();
                _pendings[tag] = pending;
            }

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

                // Timeout → vẫn gọi callback với default (để caller xử lý qua mã code trong data)
                pending.Timer = new Timer(_ =>
                {
                    if (_pendings.TryRemove(tag, out var timedOut))
                        timedOut.Fail();
                }, null, timeoutMs, Timeout.Infinite);
            }
            catch
            {
                // Gửi ngay bị lỗi → gọi callback với default luôn
                if (_pendings.TryRemove(tag, out var p))
                    p.Fail();
                else
                    callback(default(TResponse));
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var msg = e.GetMessage())
            {
                if (!_pendings.TryRemove(msg.Tag, out var pending)) return;

                pending.CancelTimeout();

                try
                {
                    using (var reader = msg.GetReader())
                    {
                        int len = reader.ReadInt32();
                        byte[] payload = reader.ReadBytes();
                        pending.Complete(payload, 0, len);
                    }
                }
                catch
                {
                    // Deserialize lỗi → vẫn trả về callback (với default)
                    pending.Fail();
                }
            }
        }

        private interface IPending
        {
            void Complete(byte[] payload, int offset, int length);
            void Fail();
            void CancelTimeout();
        }

        private class Pending<TResponse> : IPending
        {
            public Action<TResponse> Callback;
            public Timer Timer;

            public void Complete(byte[] payload, int offset, int length)
            {
                Timer?.Dispose();
                Timer = null;

                TResponse result;
                try
                {
                    var obj = MessagePackSerializer.Deserialize(
                        typeof(TResponse),
                        payload.AsSpan(offset, length).ToArray(),
                        ContractlessStandardResolver.Options);
                    result = (TResponse)obj;
                }
                catch
                {
                    result = default(TResponse);
                }

                Callback?.Invoke(result);
            }

            public void Fail()
            {
                Timer?.Dispose();
                Timer = null;
                Callback?.Invoke(default(TResponse));
            }

            public void CancelTimeout()
            {
                Timer?.Dispose();
                Timer = null;
            }
        }
    }
}
