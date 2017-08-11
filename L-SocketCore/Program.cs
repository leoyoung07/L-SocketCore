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
            socketManager.OnHeartbeatStateChange += SocketManager_OnHeartbeatStateChange;
            switch (choice)
            {
                case 1: TcpListenerProcess(); break;
                case 2: TcpClientProcess(); break;
                default:
                    break;
            }
        }

        private static void SocketManager_OnHeartbeatStateChange(Guid id, SocketClient.HEARTBEAT_STATE state)
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
                Console.WriteLine("[{0}]{1} heartbeat state {2}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), socketClient.ToString(), state);
            }
        }

        private static void SocketManager_OnClientStateChange(Guid id, SocketClient.CLIENT_STATE state)
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
                Console.WriteLine("[{0}]{1} state {2}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), socketClient.ToString(), state);
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
                //socketManager.SendText(socketClient, "hello");
                Thread.Sleep(1000);
            }
        }

        private static void TcpListenerProcess()
        {
            socketManager.Listen(port);
        }

        private static void SocketManager_onReceiveData(SocketClient client, SocketManager.MSG_TYPE type, byte[] data)
        {
            Console.WriteLine(client.ToString());
            if (type == SocketManager.MSG_TYPE.TEXT || type == SocketManager.MSG_TYPE.CMD)
            {
                Console.WriteLine("[{0}]{1}", type, Encoding.UTF8.GetString(data));
            }
            if (socketManager.AcceptedClients.ContainsKey(client.ID))
            {
                socketManager.SendText(client, "world");
            }
        }


    }
}
