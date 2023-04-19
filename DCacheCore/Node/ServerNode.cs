using DCache.Command;
using System;
using NetSockets;
using System.Text;
using System.Net;

namespace DCache
{
    public class ServerNode: Node
    {
        // TODO: use DI to get this implementation
        private NetPayloadServer SocketHandler { set; get; }

        public ServerNode(int portNum, string host = API.LOCAL_HOST)
            :base(portNum, host)
        {
            SocketHandler = new NetPayloadServer();
            SocketHandler.Start(IPAddress.Parse("127.0.0.1"), portNum);
            SocketHandler.OnReceived += new NetClientReceivedEventHandler<byte[]>(ProcessCommand);
            StartMulticast();

            try
            {
                MulticastSend($"[{API.NOTIFY}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(portNum)}','source_host':'{host}'}}]");
            } catch(Exception ee) {
            }
            
        }

        void ProcessCommand(object sender, NetClientReceivedEventArgs<byte[]> e)
        {
            string content = Encoding.UTF8.GetString(e.Data);
            CommandBuilder command = new CommandBuilder(content);

            //Console.WriteLine($"{Host}:{PortNumber} - Processcommand {command} - {value}");
            string res = commandListener(command);
            NetPayloadServer myServer = (NetPayloadServer)sender;
            myServer.DispatchTo(e.Guid, Encoding.UTF8.GetBytes(res));
        }

        public override bool SendAsync(string data)
        {
            SocketHandler.DispatchAll(Encoding.UTF8.GetBytes(data));

            return true;
        }

        public UInt64 FindSuccessorId(UInt64 id)
        {
            SocketHandler.DispatchAll(Encoding.UTF8.GetBytes($"[{ API.FIND_SUCCESSOR}:${ id.ToString()}]"));

            return 0;
        }
    }
}
