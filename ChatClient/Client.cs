﻿using System;
using System.Net; // IPAddress
using System.Net.Sockets; // TcpListener
using ChatLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ChatClient
{
    public class Client
    {
        /**
         * public const int BUFFER = 4096;
         */
        public event Action<Message> MessageFunction;

        private IPAddress serverIp;
        private ushort port;
        private string nick;

        private TcpClient client;
        private Task messageWaiting;
        private NetworkStream stream;

        public Client(string ipAddress, ushort port, string nick)
        {
            this.serverIp = IPAddress.Parse(ipAddress);
            this.port = port;
            this.nick = nick;
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

            Message message = new Message()
            {
                SendTime = DateTime.Now,
                Nick = nick
            };
            if (client.Connected)
            {
                string serverIp = GetServerIp();
                message.Text = $"Połączyłem się z [{serverIp}]";
            }
            else
                message.Text = $"Błąd połączenia";

            MessageFunction?.Invoke(message);

            messageWaiting = new Task(() => WaitForMessage());
            messageWaiting.Start();
        }

        private string GetServerIp()
        {
            IPEndPoint serverIp = (IPEndPoint)client.Client.RemoteEndPoint;
            return serverIp.ToString();
        }

        public void SendMessage(string message)
        {
            try
            {
                Message messageToSend = new Message()
                {
                    SendTime = DateTime.Now,
                    Nick = nick,
                    Text = message
                };
                var serializer = new BinaryFormatter();
                serializer.Serialize(stream, messageToSend);
                /**
                 * using (MemoryStream memStream = new MemoryStream())
                {
                    serializer.Serialize(memStream, messageToSend);
                    byte[] dataToSend = memStream.GetBuffer();
                    await stream.WriteAsync(dataToSend, 0, (int)memStream.Length);
                }*/
            }
            catch (Exception)
            {
                Message mess = new Message()
                {
                    SendTime = DateTime.Now,
                    Nick = nick,
                    Text = "Serwer zakończył działanie."
                };
                MessageFunction?.Invoke(mess);
                return;
            }
        }

        private void WaitForMessage()
        {
            stream = client.GetStream();
            while (client.Connected)
            {
                try
                {
                    /**
                     * using (MemoryStream memStream = new MemoryStream())
                    {
                        //First Aproach
                        byte[] receivedBytes = new byte[BUFFER];

                        int receivedBytesLength = await stream.ReadAsync(receivedBytes, 0, receivedBytes.Length);

                        memStream.Write(receivedBytes, 0, receivedBytesLength);
                        memStream.Seek(0, SeekOrigin.Begin);

                        // We are sending all the data in one package
                        var serializer = new BinaryFormatter();
                        Message receivedMessage = (Message)serializer.Deserialize(memStream);
                    }
                    */
                    // Second Aproach
                    var serializer = new BinaryFormatter();
                    Message receivedMessage = (Message)serializer.Deserialize(stream);

                    MessageFunction?.Invoke(receivedMessage);
                }
                catch (Exception exc)
                {
                    Message mess = new Message()
                    {
                        SendTime = DateTime.Now,
                        Nick = nick,
                        Text = "Błąd pobierania wiadomości. Serwer nie odpowiada?." + exc.Message
                    };
                    MessageFunction?.Invoke(mess);
                }
            }
            stream.Close();
        }
    }
}
