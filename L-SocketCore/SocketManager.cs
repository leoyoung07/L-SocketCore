using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace L_SocketCore
{
    public class SocketManager
    {
        public delegate void ReceiveData(SocketClient client, byte[] bytes);
        public event ReceiveData OnReceiveData;

        public delegate void AcceptClientAdd(Guid id);
        public event AcceptClientAdd OnAcceptClientAdd;

        public delegate void AcceptClientRemove(Guid id);
        public event AcceptClientRemove OnAcceptClientRemove;

        public delegate void ConnectClientAdd(Guid id);
        public event ConnectClientAdd OnConnectClientAdd;

        public delegate void ConnectClientRemove(Guid id);
        public event ConnectClientRemove OnConnectClientRemove;

        public delegate void ClientStateChange(Guid id, SocketClient.ClientState state);
        public event ClientStateChange OnClientStateChange;

        private Dictionary<Guid, SocketClient> _acceptedClients = new Dictionary<Guid, SocketClient>();
        public Dictionary<Guid, SocketClient> AcceptedClients
        {
            get
            {
                return _acceptedClients;
            }
        }

        private Dictionary<Guid, SocketClient> _connectedClients = new Dictionary<Guid, SocketClient>();
        public Dictionary<Guid, SocketClient> ConnectedClients
        {
            get
            {
                return _connectedClients;
            }
        }

        private const int INT_SIZE = 4;

        public SocketClient Connect(string hostName, int port)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(hostName, port);
                SocketClient socketClient = new SocketClient(tcpClient);
                ConnectedClients.Add(socketClient.ID, socketClient);
                OnConnectClientAdd?.Invoke(socketClient.ID);
                Thread thread = new Thread(() =>
                {
                    receiveData(socketClient);
                });
                thread.Start();
                return socketClient;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Util.WriteLog(ex.Message, "error.txt");
                tcpClient.Close();
                throw;
            }
        }

        public void Send(SocketClient socketToSend, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }
            if (socketToSend.State == SocketClient.ClientState.DISCONNECTED)
            {
                return;
            }
            Thread thread = new Thread(() => 
            {
                Int32 dataSize = (Int32)data.Length;
                NetworkStream stream = socketToSend.RemoteClient.GetStream();
                byte[] buffer = Util.ConvertInt32ToBytes(dataSize);
                stream.Write(buffer, 0, INT_SIZE);
                buffer = data;
                stream.Write(buffer, 0, dataSize);
                stream.Flush();
            });
            thread.Start();
        }

        public void SendStr(SocketClient socketToSend, string msg)
        {
            if (msg == null || msg == "")
            {
                return;
            }
            Send(socketToSend, Encoding.UTF8.GetBytes(msg));
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
                        AcceptedClients.Add(socketClient.ID, socketClient);
                        OnAcceptClientAdd?.Invoke(socketClient.ID);
                        Debug.WriteLine(socketClient.ToString() + ' ' + socketClient.State);
                        receiveData(socketClient);

                    });
                    thread.Start();
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.Message);
                Util.WriteLog(ex.Message, "error.txt");
                throw;
            }
            finally
            {
                listener.Stop();
            }

        }

        public void Disconnect(SocketClient socketToDisconnect)
        {
            //TODO Disconnect
        }

        private void receiveData(SocketClient socketClient)
        {
            NetworkStream stream = socketClient.RemoteClient.GetStream();
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
                    OnReceiveData?.Invoke(socketClient, buffer);
                }
                catch (Exception ex)
                {
                    if (socketClient.State != SocketClient.ClientState.DISCONNECTED)
                    {
                        socketClient.State = SocketClient.ClientState.DISCONNECTED;
                        OnClientStateChange?.Invoke(socketClient.ID, socketClient.State);
                        socketClient.RemoteClient.Close();
                        if (AcceptedClients.ContainsKey(socketClient.ID))
                        {
                            AcceptedClients.Remove(socketClient.ID);
                            OnAcceptClientRemove?.Invoke(socketClient.ID);
                        }
                        if (ConnectedClients.ContainsKey(socketClient.ID))
                        {
                            ConnectedClients.Remove(socketClient.ID);
                            OnConnectClientRemove?.Invoke(socketClient.ID);
                        }
                    }
                    break;
                }
            }
        }

        private void heartbeat()
        {
            //TODO heartbeat ConnectedClients
        }
    }
}
