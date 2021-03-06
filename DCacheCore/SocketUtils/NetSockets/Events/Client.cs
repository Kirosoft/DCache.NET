﻿using System;

namespace NetSockets
{
    public delegate void NetConnectedEventHandler(object sender, NetConnectedEventArgs e);
    public class NetConnectedEventArgs : EventArgs
    {

    }

    public delegate void NetDisconnectedEventHandler(object sender, NetDisconnectedEventArgs e);
    public class NetDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The reason why the client was stopped.
        /// </summary>
        public NetStoppedReason Reason { get; private set; }

        public NetDisconnectedEventArgs(NetStoppedReason reason)
        {
            Reason = reason;
        }
    }

    public delegate void NetReceivedEventHandler<T>(object sender, NetReceivedEventArgs<T> e);
    public class NetReceivedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The data received.
        /// </summary>
        public T Data { get; private set; }

        public NetReceivedEventArgs(T data)
        {
            Data = data;
        }
    }
}
