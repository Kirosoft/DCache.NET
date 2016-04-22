/*
 * ChordInstance.Maintenance.StabilizePredecessors.cs:
 * 
 *  Maintenance task to stabilize the local node's predecessor as specified by the Chord paper.
 * 
 *  This task runs every 5 seconds though that value can be tweaked as needed.
 *
 */

using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading;

namespace DCacheLib
{
    public partial class Instance
    {
        /// <summary>
        /// Maintenance task to stabilize the local node's predecessor as per the Chord paper.
        /// </summary>
        /// <param name="sender">The backgroundworker thread that this task is running on.</param>
        /// <param name="ea">Args (ignored)</param>
        private void StabilizePredecessors(object sender, DoWorkEventArgs ea)
        {
            BackgroundWorker me = (BackgroundWorker)sender;

            while (!me.CancellationPending)
            {
                try
                {
                    LocalNode.MulticastSend($"{API.FIND_PREDECCESSOR}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}");
                }
                catch (Exception e)
                {
                    //Server.Log(LogLevel.Error, "StabilizePredecessors", $"StabilizePredecessors error: {e.Message}" );
                    this.Predecessor = null;
                }

                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["StabilizePredecessorsPeriod"]));
            }
        }
    }
}
