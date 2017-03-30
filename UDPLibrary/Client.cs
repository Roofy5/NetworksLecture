using System;
using System.Net.Sockets;

namespace UDPLibrary
{
    public class Client
    {
        public event Action<string> MessageFunction;

        private string serverIp;
        private ushort port;

        private UdpClient client;

        public Client(string ipAddress, ushort port)
        {
            this.serverIp = ipAddress;
            this.port = port;
            client = new UdpClient(this.serverIp, this.port);
        }

        ~Client()
        {
            client.Close();
        }

        public async void SendMessage(string message)
        {
            byte[] dataToSend = System.Text.Encoding.ASCII.GetBytes(message);

            try
            {
                await client.SendAsync(dataToSend, dataToSend.Length);
                MessageFunction?.Invoke("Wysłałem wiadomość.");
                WaitForMessage();
            }
            catch (Exception)
            {
                MessageFunction?.Invoke("Nie udało się wysłać wiadomości");
            }
        }

        private async void WaitForMessage()
        {
            try
            {
                var result = await client.ReceiveAsync();
                string receivedMessage = System.Text.Encoding.ASCII.GetString(result.Buffer);

                MessageFunction?.Invoke($"Odebrałem: {receivedMessage}");
            }
            catch (Exception)
            {
                MessageFunction?.Invoke("Przerywam działanie");
            }
        }
    }
}
