using System;
using System.ComponentModel;
using System.Threading;
using DCache.Cluster;
using DCache.Command;

namespace DCache
{
    public partial class Instance
    {
        private void StabilizePredecessors(object sender, DoWorkEventArgs ea)
        {
            BackgroundWorker me = (BackgroundWorker)sender;

            while (!me.CancellationPending)
            {
                try
                {
                    if (Predecessor != null)
                    {
                        bool res = Predecessor.SendAsync($"[{API.PING}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                        if (!res)
                        {
                            NodeCache nc = NodeCache.Instance;
                            nc.DeleteNode(Predecessor.ID);
                            Predecessor.Close();
                            Predecessor = null;
                        }
                    }
                    LocalNode.MulticastSend($"[{API.FIND_PREDECCESSOR}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                }
                catch (Exception e)
                {
                    Log("StabilizePredecessors", $"StabilizePredecessors error: {e.Message}", LogLevel.Error  );
                    this.Predecessor = null;
                }

                Thread.Sleep(Convert.ToInt32(config["settings:StabilizePredecessorsPeriod"]));
            }
        }
    }
}
