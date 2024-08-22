using srtool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace srtool.live
{
    public class WSConnection
    {
        public string Id { get; private set; }
        public delegate void ON_DISSCONNECT(WSConnection connection);
        public delegate void ON_RECEIVED_MESSAGE(string message, WSConnection sender);
        public event ON_DISSCONNECT OnDisconnect;
        public event ON_RECEIVED_MESSAGE OnReceivedMessage;
        private WebSocket webSocket;
        public void Init(WebSocket webSocket)
        {
            Id = Guid.NewGuid().ToString();
            this.webSocket = webSocket;
        }
        public async Task RecivedLoop()
        {
            WebSocketReceiveResult result = null;
            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
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

                        if (result.MessageType != WebSocketMessageType.Text)
                        {
                            // return null;
                        }

                        // Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            string value = await reader.ReadToEndAsync();
                            try
                            {
                                OnReceivedMessage?.Invoke(value, this);
                            }
                            catch (Exception e)
                            {
                                Debug.Error(e);
                            }
                        }
                    }
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch
            {

            }

            try
            {
                OnDisconnect?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }
        public async void Send(string message)
        {
            try
            {
                await _sendMessage(webSocket, message);

            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }

        private async Task _sendMessage(WebSocket webSocket, string msg)
        {
            await SuperQueue.Run(async () =>
            {
                var encoded = Encoding.UTF8.GetBytes(msg);
                var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }, new QueueSetting() { key = $"live_connection_{GetHashCode()}" });

        }
    }
}
