using System;

namespace DCacheLib
{
    public class ServerNode: Node
    {
        // TODO: use DI to get this implementation
        private AsynchronousSocketListener SocketHandler { set; get; }

        public ServerNode(int portNum, string host = API.LOCAL_HOST)
            :base(portNum, host)
        {
            SocketHandler = new AsynchronousSocketListener();
            SocketHandler.StartListening(portNum, this);
            StartMulticast();

            MulticastSend($"{API.NOTIFY}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(portNum)}','source_host':'{host}'}}");
        }

        public void ProcessCommand(string content)
        {
            string command = content.Split('=')[0];
            string value = content.Split('=')[1];

            //Console.WriteLine($"{Host}:{PortNumber} - Processcommand {command} - {value}");
            commandListener(command, value);

        }

        public override bool Send(string data)
        {
            SocketHandler.Send(data);

            return true;
        }

        public UInt64 FindSuccessorId(UInt64 id)
        {
            SocketHandler.Send($"{ API.FIND_SUCCESSOR}:${ id.ToString()}");

            return 0;
        }
    }
}
