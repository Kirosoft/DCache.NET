using System;
using System.Collections.Concurrent;
using System.Threading;

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
        public NewSuccessor successorEventListener = null; 

        public Node Successor 
        {
            get 
            { 
                return FingerTable[0]; 
            }
            set
            {
                StartSuccessorPropertyTimer();

                if (value == null && value != FingerTable[0])
                {
                    Log("Navigation", "Setting successor to null.");
                    FingerTable[0] = value;
                    if (successorTimer != null)
                    {
                        successorTimer.Change(-1, -1);
                        successorTimer.Dispose();
                        successorTimer = null;
                    }
                    // Broadcast the event
                    successorEventListener?.Invoke(null);
                }
                else if (value != null && 
                    (FingerTable[0] == null || FingerTable[0].ID != value.ID))
                {
                    Log("SetSuccessor", $"New Successor {value}.");

                    // Tell the new successor we are now their predecessor
                    value.SendAsync($"[{API.GET_PREDECESSOR_RESPONSE}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                    FingerTable[0] = value;
                    // Broadcast the event
                    successorEventListener?.Invoke(value.ID);
                }
            }
        }

        // We expect to receive a ping from the node within a timeout period, otherwise we assume the connection is broken
        private void StartSuccessorPropertyTimer()
        {

            // stop the timer
            if (successorTimer == null)
            {
                int timerPeriod = Convert.ToInt32(config["settings:StabilizePredecessorsPeriod"]) * 3;
                successorTimer = new Timer(OnSuccessorTimerEvent, this, timerPeriod, timerPeriod);
                //successorTimer?.Change(-1, -1);
            }

        }
        private static void OnSuccessorTimerEvent(object state)
        {
            Instance instance = (Instance) state;
            NodeCache nc = NodeCache.Instance;

            if (instance.Successor != null)
            {
                Console.WriteLine($"Successor Node Timeout {instance.Successor.ID}");

                // TODO: put this back in
                //nc.DeleteNode(instance.Successor.ID);
                //instance.Successor = null;
            }
        }

        private Node m_Predecessor = null;

        public Node Predecessor
        {
            get 
            { 
                return this.m_Predecessor; 
            }
            set
            {
                StartPredecessorPropertyTimer();

                if (value == null && value != this.m_Predecessor)
                {
                    Log("Navigation", "Setting predecessor to null.");
                    this.m_Predecessor = value;
                    if (predecessorTimer != null)
                    {
                        predecessorTimer?.Change(-1, -1);
                        predecessorTimer.Dispose();
                        predecessorTimer = null;
                    }
                }
                else if (value != null && 
                    (this.m_Predecessor == null || this.m_Predecessor.ID != value.ID)) 
                {
                    Log("SetPredeccessor", $"New Predecessor {value}.");
                    this.m_Predecessor = value;
                }
            }
        }
        // We expect to receive a ping from the node within a timeout period, otherwise we assume the connection is broken
        private void StartPredecessorPropertyTimer()
        {

            if (predecessorTimer != null)
            {
                //predecessorTimer?.Change(-1,-1);
                int timerInterval = Convert.ToInt32(config["settings:StabilizePredecessorsPeriod"]) * 3;
                predecessorTimer = new Timer(OnPredecessorTimerEvent, this, timerInterval, timerInterval);
            }
        }

        private static void OnPredecessorTimerEvent(object source)
        {
            Instance instance = (Instance) source;
            NodeCache nc = NodeCache.Instance;

            if (instance.Predecessor != null)
            {
                Console.WriteLine($"Predecessor Node Timeout {instance.Predecessor.ID}");
                //nc.DeleteNode(instance.Predecessor.ID);
                //instance.Predecessor = null;
            }
        }

        public FingerTable FingerTable { get; set;  }
    }
}
