using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

namespace blueprint.srtool
{
    public class WSConnection
    {
        public event Action<WSConnection, DisconnectInfo> OnDisconnect;
        public event Action<string, WSConnection> OnReceivedMessage;
        public event Action<byte[], WSConnection> OnReceivedBytes;
        public bool IsConnected { get; private set; }
        public long udpId { get; set; }
        private WebSocket webSocket;
        public void Init(WebSocket webSocket)
        {
            this.webSocket = webSocket;
            IsConnected = true;
        }
        public async Task ReceivedLoop()
        {
            WebSocketReceiveResult result = null;
            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024 * 20]);
                bool isClose = false;

                while (!isClose)
                {
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                            if (result.CloseStatus.HasValue)
                                isClose = true;
                        }
                        while (!result.EndOfMessage);

                        if (isClose)
                            break;

                        ms.Seek(0, SeekOrigin.Begin);

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = Encoding.UTF8.GetString(ms.ToArray());
                            OnReceivedMessage?.Invoke(message, this);
                        }
                        else
                        {
                            OnReceivedBytes?.Invoke(ms.ToArray(), this);
                        }

                    }
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (WebSocketException)
            {
                // Debug.Error(e);
            }

            Disconnect();
        }
        public async Task Send(object data)
        {
            try
            {
                if (webSocket != null)
                {
                    var bytes = Encoding.UTF8.GetBytes(JObject.FromObject(data).ToString());
                    var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch
            {

            }
        }
        public async Task Send(string message)
        {
            try
            {
                if (webSocket != null)
                {
                    var bytes = Encoding.UTF8.GetBytes(message);
                    var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch
            {
            }
        }
        public async Task Send(byte[] bytes)
        {
            try
            {
                if (webSocket != null)
                {
                    var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
            catch
            {
            }
        }

        public void Disconnect()
        {
            try
            {
                this.IsConnected = false;
                webSocket.Abort();
                OnDisconnect?.Invoke(this, new DisconnectInfo() { type = "remote" });
            }
            catch
            {
            }
        }
    }
    public class DisconnectInfo
    {
        public string type { get; set; }
        public string message { get; set; }
    }
}
