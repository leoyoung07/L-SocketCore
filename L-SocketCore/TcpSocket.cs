using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace L_SocketCore
{
    public class TcpSocket
    {
        public delegate void ReadNetworkBytes(IPEndPoint endPoint, byte[] bytes);

        public event ReadNetworkBytes OnReadBytes;

        private Dictionary<Guid, SocketClient> _clients = new Dictionary<Guid, SocketClient>();
        public Dictionary<Guid, SocketClient> Clients
        {
            get
            {
                return _clients;
            }
        }

        private const int INT_SIZE = 4;

        public void Connect(string hostName, int port)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(hostName, port);
                //TODO return client
                //TODO move to send()
                NetworkStream stream = client.GetStream();
                //Thread thread = new Thread(() => 
                //{

                while (true)
                {
                    string message = "hello, world";
                    byte[] buffer = Util.ConvertInt32ToBytes((Int32)message.Length);
                    stream.Write(buffer, 0, INT_SIZE);
                    buffer = Encoding.UTF8.GetBytes(message);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();

                    Thread.Sleep(1000);
                }
                //});
                //thread.Start();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Util.WriteLog(ex.Message, "error.txt");
                throw;
            }
            finally
            {
                client.Close();
            }
        }


        public void Listen(int port, string localAddr = "0.0.0.0")
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(localAddr), port);
            try
            {
                listener.Start();
                while (true)
                {
                    //blocks here
                    TcpClient client = listener.AcceptTcpClient();
                    Thread thread = new Thread(() =>
                    {
                        SocketClient socketClient = new SocketClient(client);
                        _clients.Add(socketClient.ID, socketClient);
                        Console.WriteLine(socketClient.ToString() + ' ' + socketClient.State);
                        NetworkStream stream = socketClient.Client.GetStream();
                        while (true)
                        {
                            try
                            {
                                byte[] buffer = new byte[INT_SIZE];
                                stream.Read(buffer, 0, INT_SIZE);
                                socketClient.State = SocketClient.ClientState.DATA_SENDING;
                                Int32 dataSize = BitConverter.ToInt32(buffer, 0);
                                dataSize = IPAddress.NetworkToHostOrder(dataSize);
                                buffer = new byte[dataSize];
                                stream.Read(buffer, 0, dataSize);
                                OnReadBytes(client.Client.RemoteEndPoint as IPEndPoint, buffer);
                            }
                            catch (Exception ex)
                            {
                                if (socketClient.State != SocketClient.ClientState.DISCONNECTED)
                                {
                                    socketClient.State = SocketClient.ClientState.DISCONNECTED;
                                    Console.WriteLine("{0} is {1}", socketClient.ToString(), socketClient.State);
                                    socketClient.Client.Close();
                                    _clients.Remove(socketClient.ID);
                                }
                                break;
                            }
                        }

                    });
                    thread.Start();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Util.WriteLog(ex.Message, "error.txt");
                throw;
            }
            finally
            {
                listener.Stop();
            }

        }


    }
}
