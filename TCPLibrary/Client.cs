using System;
using System.Net; // IPAddress
using System.Net.Sockets; // TcpListener

namespace TCPLibrary
{
    public class Client
    {
        public const int BUFFER = 256;
        public event Action<string> MessageFunction;

        private IPAddress serverIp;
        private ushort port;

        private TcpClient client;

        public Client(string ipAddress, ushort port)
        {
            this.serverIp = IPAddress.Parse(ipAddress);
            this.port = port;
            client = new TcpClient();
        }

        ~Client()
        {
            client.Close();
        }

        public async void ClientConnect()
        {
            try
            {
                await client.ConnectAsync(serverIp, port);
            }
            catch (Exception)
            {
            }

            if (client.Connected)
            {
                string serverIp = GetServerIp();
                MessageFunction?.Invoke($"Połączyłem się z [{serverIp}]");
            }
            else
                MessageFunction?.Invoke("Nie udało się połączyć...");
        }

        private string GetServerIp()
        {
            IPEndPoint serverIp = (IPEndPoint)client.Client.RemoteEndPoint;
            return serverIp.ToString();
        }

        public async void SendMessage(string message)
        {
            byte[] dataToSend = new byte[BUFFER];

            try
            {
                NetworkStream stream = client.GetStream();
                dataToSend = System.Text.Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            }
            catch (Exception)
            {
                MessageFunction?.Invoke("Serwer zakończył działanie.");
                return;
            }

            MessageFunction?.Invoke($"Wysłałem: {message}");

            WaitForMessage();
        }

        private async void WaitForMessage()
        {
            byte[] receivedData = new byte[BUFFER];

            NetworkStream stream = client.GetStream();
            int downloadedBytes = await stream.ReadAsync(receivedData, 0, receivedData.Length);
            string receivedMessage = System.Text.Encoding.ASCII.GetString(receivedData, 0, downloadedBytes);

            MessageFunction?.Invoke($"Otrzymałem: {receivedMessage}");
        }
    }
}
