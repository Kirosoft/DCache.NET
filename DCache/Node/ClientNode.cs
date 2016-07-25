using System;
using NetSockets;

namespace DCache
{
    public class ClientNode: Node
    {

        public ClientNode(int portNum, string host = API.LOCAL_HOST)
            : base(portNum, host)
        {
            Console.WriteLine($"ClientNode- Creating new client for {host}:{portNum}");
            if (SocketRef == null)
            {
                SocketRef = new NetPayloadClient();
                bool connected = SocketRef.TryConnect(host, portNum);

                if (!connected)
                {
                    Console.WriteLine("Unable to connect: " + host + ", port: " + Convert.ToString(portNum));
                }
            }
        }

    }
}
