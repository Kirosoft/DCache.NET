/*
 * ChordInstance.Maintenance.UpdateFingerTable.cs:
 * 
 *  Maintenance task to keep the local node's finger table up to date.  There are myriad ways in which this particular
 *  task can be implemented (e.g. run frequency vs. number of fingers updated per execution); however, in practice this
 *  approach seemed to work fairly well where a single finger is updated every second - meaning roughly once per minute
 *  the whole finger table has been traversed.  This job has also been configured to update the entire finger table in
 *  a single go, and also to be extremely un-aggressive in keeping the finger table up-to-date - in all cases, lookup
 *  still worked and efficiency under churn didn't suffer too badly either.
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
        private int m_NextFingerToUpdate = 0;

        /// <summary>
        /// Update the local node's finger table entries on a background thread.
        /// </summary>
        /// <param name="sender">The background worker thread this task is running on.</param>
        /// <param name="ea">Args (ignored).</param>
        private void UpdateFingerTable(object sender, DoWorkEventArgs ea)
        {
            BackgroundWorker me = (BackgroundWorker)sender;

            while (!me.CancellationPending)
            {
                //try
                {
                    // update the fingers moving outwards - once the last finger
                    // has been reached, start again closest to LocalNode (0).
                    if (this.m_NextFingerToUpdate >= this.FingerTable.Length)
                    {
                        this.m_NextFingerToUpdate = 0;
                    }

                    try
                    {
                        if (FingerTable[m_NextFingerToUpdate] == null)
                        {
                            ClientNode anotherNode = LocationService.Instance.GetClusterNode(LocalNode.PortNumber);
                            //anotherNode?.FindSuccessor(LocalNode.ID);
                        }
                        else
                        {
                            // Node validity is checked by findSuccessor
                            //this.FingerTable.Fingers[this.m_NextFingerToUpdate] = FindSuccessor(this.FingerTable.Start[this.m_NextFingerToUpdate]);
                        }
                    }
                    catch (Exception e)
                    {
                        Log("FingerTable", "Unable to update Successor for start value {this.FingerTable.StartValues[this.m_NextFingerToUpdate]} ({e.Message}).", LogLevel.Error );
                    }

                    this.m_NextFingerToUpdate += 1;
                }
                //catch (Exception e)
                //{
                //    // (overly safe here)
                //    Log(LogLevel.Error, "Maintenance", $"Error occured during UpdateFingerTable ({e.Message})" );
                //}

                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["FingerTableUpdatePeriod"]));
            }
        }
    }
}
