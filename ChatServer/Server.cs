using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
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
        private object sendLocker;

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
            sendLocker = new object();
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
            Task task = new Task(ClientService(client), client);
            tasks.Add(task);

            task.Start();

            //tasks.Add(Task.Factory.StartNew(ClientService, client));
            //tasks.Add(new Task(ClientService(client), client));
        }
        private Action<object> ClientService(object client)
        {
            Action<object> clientFunction = /*async*/ argClient =>
            {
                TcpClient _client = argClient as TcpClient;
                byte[] receivedBytes = new byte[BUFFER_SIZE];
                try
                {
                    using (NetworkStream stream = _client.GetStream())
                    {
                        while (_client.Connected)
                        {
                            //int downloadedBytes = await stream.ReadAsync(receivedBytes, 0, receivedBytes.Length);
                            int downloadedBytes = stream.Read(receivedBytes, 0, receivedBytes.Length);
                            UpdateStatus?.Invoke($"DEBUG: Pobrałem {downloadedBytes} bajtów");
                            //for(int i = 0; i < downloadedBytes; i++)
                            //    UpdateStatus?.Invoke($"{i}   {receivedBytes[i].ToString()}");

                            SendToAllClients(receivedBytes, downloadedBytes);
                        }
                    }
                }
                catch (Exception exc)
                {
                    UpdateStatus($"#Something went wrong. {exc.Message}");
                }
            };
            return clientFunction;
        }
        private void SendToAllClients(byte[] message, int bytes)
        {
            lock (sendLocker)
            {
                foreach (TcpClient client in clients)
                    SendToSingleClient(client, message, bytes);
            }
        }
        private async void SendToSingleClient(TcpClient client, byte[] message, int bytes)
        {
            if (!client.Connected)
                return;
            using (NetworkStream stream = client.GetStream())
            {
                byte[] toSend = new byte[BUFFER_SIZE];
                message.CopyTo(toSend, 0);

                /*await*/ //stream.Write(messageLength, 0, 4); // int = 4 bajty
                await stream.WriteAsync(toSend, 0, bytes); //+4
            }
        }

    }
}
