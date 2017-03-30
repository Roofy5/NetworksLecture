using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using ChatLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ChatServer
{
    class Server
    {
        public const int BUFFER_SIZE = 4096;
        public event Action<string> UpdateStatus;

        private bool isWorking;
        private IPAddress serverAddress;
        private ushort port;

        private TcpListener server;
        private List<TcpClient> clients;
        private List<Task> tasks;

        public bool IsWorking
        {
            get { return isWorking; }
            set { isWorking = value; }
        }

        public Server(IPAddress address, ushort port)
        {
            this.serverAddress = address;
            this.port = port;
            server = new TcpListener(serverAddress, port);
            clients = new List<TcpClient>();
            tasks = new List<Task>();
        }

        public void StartServer()
        {
            server.Start();
            isWorking = true;
            UpdateStatus("Start");
            WaitForClients();
        }
        private async void WaitForClients()
        {
            while (isWorking)
            {
                var newClient = await server.AcceptTcpClientAsync();
                AddNewClient(newClient);
            }
        }
        private void AddNewClient(TcpClient client)
        {
            if (clients.Contains(client))
                return;
            clients.Add(client);
            UpdateStatus?.Invoke($"New client connected. {GetClientsDetails(client)}");
            AddClientTask(client);
        }
        private string GetClientsDetails(TcpClient client)
        {
            IPEndPoint clientsAddress = client.Client.RemoteEndPoint as IPEndPoint;
            return $"[{clientsAddress.Address}]:[{clientsAddress.Port}]";
        }
        private void AddClientTask(TcpClient client)
        {
            tasks.Add(Task.Factory.StartNew(ClientService, client));
            //tasks.Add(new Task(ClientService(client), client));
        }
        private Action<object> ClientService(object client)
        {
            Action<object> clientFunction = async argClient => 
            {
                TcpClient _client = argClient as TcpClient;
                byte[] receivedBytes = new byte[BUFFER_SIZE];
                while (_client.Connected)
                {
                    try
                    {
                        using (NetworkStream stream = _client.GetStream())
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            int downloadedBytes = await stream.ReadAsync(receivedBytes, 0, receivedBytes.Length);
                            memStream.Write(receivedBytes, 0, downloadedBytes);
                            var serializer = new BinaryFormatter();
                            Message newMessage = (Message)serializer.Deserialize(memStream);
                            // Send to all
                        }
                    }
                    catch (Exception exc)
                    {
                        UpdateStatus($"#Something went wrong. {exc.Message}");
                    }
                }       
            };
            return clientFunction;
        }

    }
}
