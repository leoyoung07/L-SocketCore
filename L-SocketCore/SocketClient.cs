using System;
using System.Net;
using System.Net.Sockets;

namespace L_SocketCore
{
    public class SocketClient
    {
        public SocketClient(TcpClient client, CLIENT_STATE state = CLIENT_STATE.CONNECTED)
        {
            ID = Guid.NewGuid();
            RemoteClient = client;
            State = state;
        }

        public enum CLIENT_STATE
        {
            CONNECTED,
            DISCONNECTED,
            DATA_SEND_BEGIN,
            DATA_SEND_END,
            DATA_RECEIVE_BEGIN,
            DATA_RECEIVE_END
        }

        public enum HEARTBEAT_STATE
        {
            INIT,
            PING_SEND,
            PING_RECEIVE,
            PONG_SEND,
            PONG_RECEIVE,
            PENDING
        }

        public Guid ID
        {
            get;
        }

        public TcpClient RemoteClient
        {
            get;
        }

        public delegate void StateChange(Guid id, CLIENT_STATE state);
        public event StateChange OnStateChange;

        private CLIENT_STATE _state;
        public CLIENT_STATE State
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

        public delegate void HeartbeatStateChange(Guid id, HEARTBEAT_STATE state);
        public event HeartbeatStateChange OnHeartbeatStateChange;
        private HEARTBEAT_STATE _heartbeatState;
        public HEARTBEAT_STATE HeartbeatState
        {
            get
            {
                return _heartbeatState;
            }
            set
            {
                _heartbeatState = value;
                OnHeartbeatStateChange?.Invoke(ID, _heartbeatState);
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
