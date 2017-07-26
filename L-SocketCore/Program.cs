using System;
using System.Net;
using System.Text;
using System.Threading;

namespace L_SocketCore
{
    class Program
    {
        private static string hostName = "192.168.1.107";
        private static int port = 7269;

        private static SocketManager socketManager;

        static void Main(string[] args)
        {
            Console.WriteLine("1. TCP Listener");
            Console.WriteLine("2. TCP Client");

            int choice = int.Parse(Console.ReadLine());

            socketManager = new SocketManager();
            socketManager.OnReceiveData += SocketManager_onReceiveData;
            socketManager.OnAcceptClientAdd += SocketManager_OnAcceptClientAdd;
            socketManager.OnAcceptClientRemove += SocketManager_OnAcceptClientRemove;
            socketManager.OnConnectClientAdd += SocketManager_OnConnectClientAdd;
            socketManager.OnConnectClientRemove += SocketManager_OnConnectClientRemove;
            socketManager.OnClientStateChange += SocketManager_OnClientStateChange;
            socketManager.OnHeartbeatReceive += SocketManager_OnHeartbeatReceive;
            switch (choice)
            {
                case 1: TcpListenerProcess(); break;
                case 2: TcpClientProcess(); break;
                default:
                    break;
            }
        }

        private static void SocketManager_OnHeartbeatReceive(SocketClient client, byte[] bytes)
        {
            Console.WriteLine("[Heartbeat]{0}: {1}", client, Encoding.UTF8.GetString(bytes));
        }

        private static void SocketManager_OnClientStateChange(Guid id, SocketClient.ClientState state)
        {
            SocketClient socketClient = null;
            if (socketManager.AcceptedClients.ContainsKey(id))
            {
                socketClient = socketManager.AcceptedClients[id];
            }
            if (socketManager.ConnectedClients.ContainsKey(id))
            {
                socketClient = socketManager.ConnectedClients[id];
            }
            if (socketClient != null)
            {
                Console.WriteLine("{0} is {1}", socketClient.ToString(), socketClient.State);
            }
            
        }

        private static void SocketManager_OnConnectClientRemove(Guid id)
        {
            Console.WriteLine("{0} is removed from ConnectedClients", id);
        }

        private static void SocketManager_OnConnectClientAdd(Guid id)
        {
            Console.WriteLine("{0} is added to ConnectedClients", id);
        }

        private static void SocketManager_OnAcceptClientRemove(Guid id)
        {
            Console.WriteLine("{0} is removed from AcceptedClients", id);
        }

        private static void SocketManager_OnAcceptClientAdd(Guid id)
        {
            Console.WriteLine("{0} is added to AcceptedClients", id);
        }

        private static void TcpClientProcess()
        {
            SocketClient socketClient = socketManager.Connect(hostName, port);
            while (true)
            {
                socketManager.SendStr(socketClient, "hello");
                Thread.Sleep(1000);
            }
        }

        private static void TcpListenerProcess()
        {
            socketManager.Listen(port);
        }

        private static void SocketManager_onReceiveData(SocketClient client, byte[] data)
        {
            Console.WriteLine(client.ToString());
            Console.WriteLine(Encoding.UTF8.GetString(data));
            if (socketManager.AcceptedClients.ContainsKey(client.ID))
            {
                socketManager.SendStr(client, "world");
            }
        }


    }
}
