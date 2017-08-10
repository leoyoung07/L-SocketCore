using System;
using System.Net;
using System.Net.Sockets;

namespace L_SocketCore
{
    public class SocketClient
    {
        public SocketClient(TcpClient client, ClientState state = ClientState.CONNECTED)
        {
            ID = Guid.NewGuid();
            RemoteClient = client;
            State = state;
        }

        public enum ClientState
        {
            CONNECTED,
            DISCONNECTED,
            DATA_SEND_BEGIN,
            DATA_SEND_END,
            HEARTBEAT_SEND,
            HEARTBEAT_RECEIVE,
            HEARTBEAT_PENDING
        }

        public Guid ID
        {
            get;
        }

        public TcpClient RemoteClient
        {
            get;
        }

        public delegate void StateChange(Guid id, ClientState state);
        public event StateChange OnStateChange;

        private ClientState _state;
        public ClientState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                OnStateChange?.Invoke(ID, _state);
            }
        }

        public override string ToString()
        {
            if (RemoteClient == null)
            {
                return "";
            }
            return (RemoteClient.Client.RemoteEndPoint as IPEndPoint).ToString();
        }
    }
}
