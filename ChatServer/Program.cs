using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress myAddress = IPAddress.Parse("127.0.0.1");
            ushort myPort = 33000;
            Server myServer = new Server(myAddress, myPort);
            myServer.UpdateStatus += Message;

            myServer.StartServer();
            while (true)
            { }
            //Console.ReadKey();
        }

        static void Message(string message)
        {
            if (message.StartsWith("#"))
            {

            }
            else
            {

            }

            Console.WriteLine(message);
        }
    }
}
