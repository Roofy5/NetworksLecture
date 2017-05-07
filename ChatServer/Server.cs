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
        private Dictionary<TcpClient, NetworkStream> clients;
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
            clients = new Dictionary<TcpClient, NetworkStream>();
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
            try
            {
                clients.Add(client, client.GetStream());
            }
            catch (Exception)
            {
                //This client is already connected
                return;
            }
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
            Task task = new Task(ClientService(), client);
            tasks.Add(task);

            task.Start();
        }
        private Action<object> ClientService()
        {
            Action<object> clientFunction = async argClient =>
            {
                TcpClient _client = argClient as TcpClient;
                NetworkStream stream = clients[_client];
                byte[] receivedBytes = new byte[BUFFER_SIZE];
                try
                {
                    while (_client.Connected)
                    {
                        int downloadedBytes = await stream.ReadAsync(receivedBytes, 0, receivedBytes.Length);
                        UpdateStatus?.Invoke($"DEBUG: Pobrałem {downloadedBytes} bajtów");
                        
                        SendToAllClients(receivedBytes, downloadedBytes);
                    }   
                }
                catch (Exception)
                {
                    UpdateStatus($"Client disconnected. {GetClientsDetails(_client)}");
                }
            };
            return clientFunction;
        }
        private void SendToAllClients(byte[] message, int bytes)
        {
            lock (sendLocker)
            {
                foreach (TcpClient client in clients.Keys)
                    SendToSingleClient(client, message, bytes);
            }
        }
        private async void SendToSingleClient(TcpClient client, byte[] message, int bytes)
        {
            if (!client.Connected)
                return;
            
            byte[] toSend = new byte[BUFFER_SIZE];
            message.CopyTo(toSend, 0);

            NetworkStream stream = clients[client];
            await stream.WriteAsync(toSend, 0, bytes);
        }

    }
}
