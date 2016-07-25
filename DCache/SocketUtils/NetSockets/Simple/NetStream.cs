using System.Net;
using System.Net.Sockets;

namespace NetSockets
{
    public class NetStream : NetBaseStream<byte[]>
    {
        public NetStream(NetworkStream stream, EndPoint endpoint)
            : base(stream, endpoint)
        {
        }

        public override void Send(byte[] data)
        {
            base.SendRaw(data);
        }
        public override void SendAsync(byte[] data)
        {
            base.SendAsyncRaw(data);
        }

        protected override void ReceivedRaw(byte[] bytes)
        {
            RaiseOnReceived(bytes);
        }
    }
}
