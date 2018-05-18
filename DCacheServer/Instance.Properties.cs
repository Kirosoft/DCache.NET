using System;
using System.Collections.Concurrent;
using System.Threading;
using DCache.Cluster;
using DCache.Command;

namespace DCache
{
    public partial class Instance
    {
        public ServerNode LocalNode { get; set; }
        public string Host => LocalNode.Host;
        public int Port => LocalNode.PortNumber;
        public UInt64 ID => LocalNode.ID;
        public bool departing = false;
        private Timer predecessorTimer = null;
        private Timer successorTimer = null;
        private ConcurrentDictionary<string, object> keyStore = new ConcurrentDictionary<string, object>();
        private ConcurrentDictionary<string, object> backupKeyStore = new ConcurrentDictionary<string, object>();
        public delegate void NewSuccessor(UInt64? successorID);
        public delegate void NewPredecessor(UInt64? predecessorID);
        public NewSuccessor successorEventListener = null;
        public NewPredecessor predecessorEventListener = null;
        private Node m_Successor = null;
        private Node m_Predecessor = null;

        public Node Successor 
        {
            get 
            { 
                return m_Successor; 
            }
            set
            {

                if (value == null)
                {
                    Log("SetSuccessor", "Setting successor to null.");
                    m_Successor = value;
                    if (successorTimer != null)
                    {
                        successorTimer.Change(-1, -1);
                        successorTimer.Dispose();
                        successorTimer = null;
                    }
                    // Broadcast the event
                    successorEventListener?.Invoke(null);
                }
                else if (m_Successor == null || m_Successor.ID != value.ID)
                {
                    Log("SetSuccessor", $"New Successor {value}.");

                    // Tell the new successor we are now their predecessor
                    value.SendAsync($"[{API.GET_PREDECESSOR_RESPONSE}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                    m_Successor = value;
                    // Broadcast the event
                    successorEventListener?.Invoke(value.ID);
                    StartSuccessorPropertyTimer();
                }
                else if (value.ID == m_Successor.ID)
                {
                    //Log("SetSuccessor", "Setting successor to ourselves.");
                    StartSuccessorPropertyTimer();

                }
                else
                {
                    Log("SetSuccessor", "Unexpected successor event.");

                }
            }
        }

        // We expect to receive a ping from the node within a timeout period, otherwise we assume the connection is broken
        private void StartSuccessorPropertyTimer()
        {
            if (successorTimer != null)
            {
                successorTimer.Change(-1, -1);
                successorTimer.Dispose();
            } 

            int timerPeriod = Convert.ToInt32(config["settings:StabilizePredecessorsPeriod"]) * 3;
            successorTimer = new Timer(OnSuccessorTimerEvent, this, timerPeriod, timerPeriod);
        }

        private static void OnSuccessorTimerEvent(object state)
        {
            Instance instance = (Instance) state;
            NodeCache nc = NodeCache.Instance;

            if (instance.Successor != null)
            {
                Console.WriteLine($"Successor Node Timeout {instance.Successor.ID}");

                nc.DeleteNode(instance.Successor.ID);
                instance.Successor = null;
            }
        }


        public Node Predecessor
        {
            get 
            { 
                return this.m_Predecessor; 
            }
            set
            {

                if (value == null)
                {
                    Log("SetPredeccessor", "Setting predecessor to null.");
                    this.m_Predecessor = null;
                    if (predecessorTimer != null)
                    {
                        predecessorTimer?.Change(-1, -1);
                        predecessorTimer.Dispose();
                        predecessorTimer = null;
                    }
                }
                else if (this.m_Predecessor == null || this.m_Predecessor.ID != value.ID) 
                {
                    Log("SetPredeccessor", $"New Predecessor {value}.");
                    this.m_Predecessor = value;

                    // Tell the new successor we are now their predecessor
                    value.SendAsync($"[{API.GET_SUCCESSOR_RESPONSE}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                    // Broadcast the event
                    predecessorEventListener?.Invoke(value.ID);
                    StartPredecessorPropertyTimer();

                }
                else if (this.m_Predecessor.ID == value.ID)
                {
                    //Log("SetPredeccessor", $"Existing Predecessor {value}.");
                    //this.m_Predecessor = value;
                    StartPredecessorPropertyTimer();
                }
                else
                {
                    Log("SetPredecessor", "Unexpected precessor event.");

                }
            }
        }
        // We expect to receive a ping from the node within a timeout period, otherwise we assume the connection is broken
        private void StartPredecessorPropertyTimer()
        {

            if (predecessorTimer != null)
            {
                predecessorTimer?.Change(-1, -1);
                predecessorTimer.Dispose();
                predecessorTimer = null;
            }
            int timerInterval = Convert.ToInt32(config["settings:StabilizePredecessorsPeriod"]) * 3;
            predecessorTimer = new Timer(OnPredecessorTimerEvent, this, timerInterval, timerInterval);
        }

        private static void OnPredecessorTimerEvent(object source)
        {
            Instance instance = (Instance) source;
            NodeCache nc = NodeCache.Instance;

            if (instance.Predecessor != null)
            {
                Console.WriteLine($"Predecessor Node Timeout {instance.Predecessor.ID}");
                nc.DeleteNode(instance.Predecessor.ID);
                instance.Predecessor = null;
            }
        }
    }
}
