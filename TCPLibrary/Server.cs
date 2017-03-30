using System;
using System.Net; // IPAddress
using System.Net.Sockets; // TcpListener

namespace TCPLibrary
{
    public class Server
    {
        public const int BUFFER = 256;
        public event Action<string> MessageFunction;

        private IPAddress serverIp;
        private ushort port;

        private TcpListener listener;
        private TcpClient client;

        public Server(string ipAddress, ushort port)
        {
            this.serverIp = IPAddress.Parse(ipAddress);
            this.port = port;

            listener = new TcpListener(serverIp, port);
            client = new TcpClient();
        }

        ~Server()
        {
            client?.Close();
            listener?.Stop();
        }

        public void StartListening()
        {
            listener.Start();
            MessageFunction?.Invoke("Rozpocząłem nasłuch");
        }

        public async void WaitForClient()
        {
            client = await listener.AcceptTcpClientAsync();
            //client = listener.AcceptTcpClient();
            string clientIp = GetClientIp();

            MessageFunction?.Invoke($"Klient podłączony. [{clientIp}]");

            WaitForMessage();
        }

        private string GetClientIp()
        {
            IPEndPoint clientIp = (IPEndPoint)client.Client.RemoteEndPoint;
            return clientIp.ToString();
        }

        private async void WaitForMessage()
        {
            bool wait = true;
            byte[] receivedData = new byte[BUFFER];
            int downloadedBytes = 0;

            while (wait)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    downloadedBytes = await stream.ReadAsync(receivedData, 0, receivedData.Length);
                    string receivedMessage = System.Text.Encoding.ASCII.GetString(receivedData, 0, downloadedBytes);
                    
                    MessageFunction?.Invoke($"Otrzymałem: {receivedMessage}");

                    string newMessage = DoSomeWork(receivedMessage);
                    SendMessage(newMessage);
                }
                catch (Exception)
                {
                    MessageFunction?.Invoke("Klient się odłączył");
                    wait = false;
                }
            }
        }

        private string DoSomeWork(string message)
        {
            return message.ToUpper();
        }

        private async void SendMessage(string message)
        {
            byte[] dataToSend = new byte[BUFFER];

            NetworkStream stream = client.GetStream();
            dataToSend = System.Text.Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);

            MessageFunction?.Invoke($"Wysłałem: {message}");
        }
    }
}
