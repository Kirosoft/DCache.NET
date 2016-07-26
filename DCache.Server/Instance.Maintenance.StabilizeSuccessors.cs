using System;
using System.ComponentModel;
using System.Threading;
using DCache.Cluster;
using DCache.Command;

namespace DCache
{
    public partial class Instance
    {
        private void StabilizeSuccessors(object sender, DoWorkEventArgs ea)
        {
            BackgroundWorker me = (BackgroundWorker)sender;

            while (!me.CancellationPending)
            {
                try
                {
                    if(Successor != null)
                    {
                        bool res = Successor.SendAsync($"[{API.PING}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                        if (!res)
                        {
                            NodeCache nc = NodeCache.Instance;
                            nc.DeleteNode(Successor.ID);
                            Successor.Close();
                            Successor = null;
                        }
                    }
                    LocalNode.MulticastSend($"[{API.FIND_SUCCESSOR}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                    //Log("StabilizeSuccessors", "Ring consistency error, Re-Joining Chord ring.", LogLevel.Error);
                }
                catch (Exception e)
                {
                    Log("Maintenance", $"Error occured during StabilizeSuccessors ({e.Message})", LogLevel.Error );
                }

                Thread.Sleep(Convert.ToInt32(config["settings:StabilizeSuccessorsPeriod"]));
            }
        }

       
    }
}
