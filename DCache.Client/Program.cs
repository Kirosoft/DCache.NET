using DCache;
using DCache.Command;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace DCache.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            LocationService locationService = LocationService.Instance;
            ClientNode clusterNode = locationService.GetClusterNode();
            clusterNode.SocketRef.OnReceived += SocketRef_OnReceived;
            CommandBuilder commandBuilder = new CommandBuilder();
            commandBuilder.AddCommand(CommandBuilder.PUT_KEYS)
                          .AddKey("zzzkey18", "{'data':'data5','map_name':'_system','partition_id':'null'}")
                          .AddKey("zzzaak19", "{'data':'data7','map_name':'_system','partition_id':'null'}");

            clusterNode.Send(commandBuilder.ToString());

            Console.WriteLine("Data sent");

            commandBuilder.Init();
            commandBuilder.AddCommand(CommandBuilder.GET_KEYS)
                          .AddKey("zzzkey18", "{'map_name':'_system','partition_id':'null'}")
                          .AddKey("zzzaak19", "{'map_name':'_system','partition_id':'null'}");

            clusterNode.SendAsync(commandBuilder.ToString());
            clusterNode.Close();
            Console.ReadKey();

        }

        private static void SocketRef_OnReceived(object sender, NetSockets.NetReceivedEventArgs<byte[]> e)
        {
            Console.WriteLine("Data revieved: " + Encoding.UTF8.GetString(e.Data));
        }
    }
}
