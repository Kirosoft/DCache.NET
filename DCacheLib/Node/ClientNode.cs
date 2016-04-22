using System;

namespace DCacheLib
{
    public class ClientNode: Node
    {
        AsynchronousClient socketClient = new AsynchronousClient();

        public ClientNode(int portNum, string host = API.LOCAL_HOST)
            : base(portNum, host)
        {
            Console.WriteLine($"ClientNode- Creating new client for {host}:{portNum}");
            SocketRef = AsynchronousClient.StartClient(portNum, host);
        }

    }
}
