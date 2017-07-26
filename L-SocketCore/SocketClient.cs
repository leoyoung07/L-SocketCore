﻿using System;
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
            DATA_SENDING
        }

        public Guid ID
        {
            get;
        }

        public TcpClient RemoteClient
        {
            get;
        }

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
