using System;
using System.Net;
using System.Text;

namespace L_SocketCore
{
    class Program
    {
        private static string hostName = "192.168.1.107";
        private static int port = 7269;
        static void Main(string[] args)
        {
            Console.WriteLine("1. TCP Listener");
            Console.WriteLine("2. TCP Client");

            int choice = int.Parse(Console.ReadLine());

            TcpSocket tcpSocket = new TcpSocket();
            tcpSocket.OnReadBytes += Program_onReadNetworkBytes;
            switch (choice)
            {
                case 1: TcpListenerProcess(tcpSocket); break;
                case 2: TcpClientProcess(tcpSocket); break;
                default:
                    break;
            }
        }

        private static void TcpClientProcess(TcpSocket tcpSocket)
        {
            tcpSocket.Connect(hostName, port);
        }

        private static void TcpListenerProcess(TcpSocket tcpSocket)
        {
            tcpSocket.Listen(port);
        }

        private static void Program_onReadNetworkBytes(IPEndPoint endPoint, byte[] bytes)
        {
            Console.WriteLine(endPoint.ToString());
            Console.WriteLine(Encoding.UTF8.GetString(bytes));
        }


    }
}
