using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace NetSockets
{
    public class NetStringStream : NetBaseStream<string>
    {
        public NetStringStream(NetworkStream stream, EndPoint endpoint)
            : base(stream, endpoint)
        {
        }

        public override void Send(string data)
        {
            base.SendRaw(System.Text.Encoding.UTF8.GetBytes(data));
        }

        public override void SendAsync(string data)
        {
            base.SendAsyncRaw(System.Text.Encoding.UTF8.GetBytes(data));
        }

        protected override void ReceivedRaw(byte[] bytes)
        {
            base.RaiseOnReceived(System.Text.Encoding.UTF8.GetString(bytes));
        }
    }
}
