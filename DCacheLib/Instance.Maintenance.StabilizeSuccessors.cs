/*
 * ChordInstance.Maintenance.StabilizeSuccessors.cs:
 * 
 *  Maintenance task to stabilize successors per the Chord paper.  The algorithm here doesn't deviate too
 *  too much from that specified in the Chord paper, though there is a little extra handling for error
 *  cases, etc.
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
        /// Maintenance task to ensure that the local node has valid successor node.  Roughly equivalent
        /// to what is called out in the Chord paper.
        /// </summary>
        /// <param name="sender">The worker thread the task is running on.</param>
        /// <param name="ea">Args (ignored here).</param>
        private void StabilizeSuccessors(object sender, DoWorkEventArgs ea)
        {
            BackgroundWorker me = (BackgroundWorker)sender;

            while (!me.CancellationPending)
            {
                try
                {
                    LocalNode.MulticastSend($"{API.FIND_SUCCESSOR}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}");
                    //Log("StabilizeSuccessors", "Ring consistency error, Re-Joining Chord ring.", LogLevel.Error);
                }
                catch (Exception e)
                {
                    Log("Maintenance", $"Error occured during StabilizeSuccessors ({e.Message})", LogLevel.Error );
                }

                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["StabilizeSuccessorsPeriod"]));
            }
        }

       
    }
}
