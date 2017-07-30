using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace L_SocketCore
{
    //TODO server side heartbeat detection
    public class SocketManager
    {
        public delegate void ReceiveData(SocketClient client, MSG_TYPE type, byte[] bytes);
        public event ReceiveData OnReceiveData;

        public delegate void HeartbeatSend(SocketClient client, MSG_TYPE type);
        public event HeartbeatSend OnHeartbeatSend;

        public delegate void HeartbeatReceive(SocketClient client, MSG_TYPE type);
        public event HeartbeatReceive OnHeartbeatReceive;

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

        public enum MSG_TYPE
        {
            PING,
            PONG,
            TEXT,
            BIN,
            CMD
        }

        public SocketManager()
        {
            heartbeat();
        }

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

        public void Send(SocketClient socketToSend, MSG_TYPE type, byte[] data)
        {
            if (socketToSend.State == SocketClient.ClientState.DISCONNECTED)
            {
                return;
            }
            Thread thread = new Thread(() => 
            {
                NetworkStream stream = socketToSend.RemoteClient.GetStream();
                //write type
                byte[] buffer = Util.ConvertInt32ToBytes((Int32)type);
                stream.Write(buffer, 0, INT_SIZE);
                //write length
                Int32 dataSize = 0;
                if (data != null)
                {
                    dataSize = (Int32)data.Length;
                }
                buffer = Util.ConvertInt32ToBytes(dataSize);
                stream.Write(buffer, 0, INT_SIZE);
                //write data
                if (data != null)
                {
                    buffer = data;
                    stream.Write(buffer, 0, dataSize);
                }
                stream.Flush();
            });
            thread.Start();
        }

        public void SendText(SocketClient socketToSend, string msg)
        {
            if (msg == null || msg == "")
            {
                return;
            }
            Send(socketToSend, MSG_TYPE.TEXT, Encoding.UTF8.GetBytes(msg));
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
                    //read type
                    stream.Read(buffer, 0, INT_SIZE);
                    socketClient.State = SocketClient.ClientState.DATA_SENDING;
                    Int32 typeInt = BitConverter.ToInt32(buffer, 0);
                    MSG_TYPE type = (MSG_TYPE)IPAddress.NetworkToHostOrder(typeInt);
                    //read length
                    stream.Read(buffer, 0, INT_SIZE);
                    Int32 dataSize = BitConverter.ToInt32(buffer, 0);
                    dataSize = IPAddress.NetworkToHostOrder(dataSize);
                    //read data
                    if (dataSize > 0)
                    {
                        buffer = new byte[dataSize];
                        stream.Read(buffer, 0, dataSize);
                    }
                    else
                    {
                        buffer = null;
                    }
                    dataHandler(socketClient, type, buffer);
                    socketClient.State = SocketClient.ClientState.CONNECTED;
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

        private void dataHandler(SocketClient socketClient, MSG_TYPE type, byte[] buffer)
        {
            switch (type)
            {
                case MSG_TYPE.PING:
                    OnHeartbeatReceive?.Invoke(socketClient, type);
                    Send(socketClient, MSG_TYPE.PONG, null);
                    OnHeartbeatSend?.Invoke(socketClient, MSG_TYPE.PONG);
                    break;
                case MSG_TYPE.PONG:
                    OnHeartbeatReceive?.Invoke(socketClient, type);
                    break;
                case MSG_TYPE.TEXT:
                    OnReceiveData?.Invoke(socketClient, type, buffer);
                    break;
                case MSG_TYPE.BIN:
                    break;
                case MSG_TYPE.CMD:
                    break;
                default:
                    OnReceiveData?.Invoke(socketClient, type, buffer);
                    break;
            }
        }

        private void heartbeat()
        {
            //heartbeat ConnectedClients
            int interval = 5 * 1000;
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    foreach (var item in ConnectedClients)
                    {
                        try
                        {
                            var client = item.Value;
                            Send(client, MSG_TYPE.PING, null);
                            OnHeartbeatSend?.Invoke(client, MSG_TYPE.PING);
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(ex.Message);
                            Util.WriteLog(ex.Message, "error.txt");
                        }

                    }
                    Thread.Sleep(interval);
                }
            });
            thread.Start();
        }
    }
}
