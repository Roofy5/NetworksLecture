using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Net;

namespace UDPLibrary
{
    public class Server
    {
        public event Action<string> MessageFunction;

        private IPEndPoint serverPoint;
        private ushort port;

        private UdpClient server;
        private IPEndPoint client;

        public Server(ushort port)
        {
            this.port = port;
            this.serverPoint = new IPEndPoint(IPAddress.Any, this.port);
            server = new UdpClient(serverPoint);
        }

        ~Server()
        {
            server?.Close();
        }

        public async void WaitForMessage()
        {
            bool wait = true;

            while (wait)
            {
                try
                {
                    MessageFunction?.Invoke("Ozcekuję na wiadomości...");
                    //receivedData = server.Receive(ref client);
                    var result = await server.ReceiveAsync();
                    client = result.RemoteEndPoint;
                    string receivedMessage = System.Text.Encoding.ASCII.GetString(result.Buffer);

                    MessageFunction?.Invoke($"Wiadomość z {client.ToString()}");
                    MessageFunction?.Invoke($"Treść: {receivedMessage}");

                    string messageToSendBack = DoSomeWork(receivedMessage);
                    SendBack(messageToSendBack);
                }
                catch (Exception)
                {
                    wait = false;
                    MessageFunction?.Invoke("Przerywam działanie");
                }
            }
        }

        private string DoSomeWork(string message)
        {
            return message.ToLower();
        }

        private async void SendBack(string message)
        {
            byte[] dataToSend = System.Text.Encoding.ASCII.GetBytes(message);
            await server.SendAsync(dataToSend, dataToSend.Length, client);
        }
    }
}
