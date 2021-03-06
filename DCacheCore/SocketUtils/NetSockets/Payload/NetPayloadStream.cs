﻿using System.Net;
using System.Net.Sockets;

namespace NetSockets
{
    public class NetPayloadStream : NetBasePayloadStream<byte[]>
    {
        public NetPayloadStream(NetworkStream stream, EndPoint endpoint)
            : base(stream, endpoint)
        {
        }

        public override void Send(byte[] data)
        {
            SendPayload(data);
        }
        public override void SendAsync(byte[] data)
        {
            SendAsyncPayload(data);
        }

        protected override void ReceivedPayload(byte[] data)
        {
            RaiseOnReceived(data);
        }
    }
}
