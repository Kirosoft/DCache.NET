using System;
using System.Configuration;
using System.Timers;

namespace DCacheLib
{
    public partial class Instance
    {
        public ServerNode LocalNode { get; set; }
        public string Host => LocalNode.Host;
        public int Port => LocalNode.PortNumber;
        public UInt64 ID => LocalNode.ID;
        public bool departing = false;
        private DataTimer predecessorTimer = null;
        private DataTimer successorTimer = null;

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
                    successorTimer?.Stop();
                }
                else if (value != null && 
                    (FingerTable[0] == null || FingerTable[0].ID != value.ID))
                {
                    Log("SetSuccessor", $"New Successor {value}.");
                    // Tell the new successor we are now their predecessor
                    value.Send($"{API.GET_PREDECESSOR_RESPONSE}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}");
                    FingerTable[0] = value;
                }
            }
        }

        private void StartSuccessorPropertyTimer()
        {
            successorTimer?.Stop();
            successorTimer = new DataTimer();
            successorTimer.InstanceData = this;
            successorTimer.Elapsed += new ElapsedEventHandler(OnSuccessorTimerEvent);
            successorTimer.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["StabilizePredecessorsPeriod"]) * 3;
            successorTimer.Enabled = true;
        }
        private static void OnSuccessorTimerEvent(object source, ElapsedEventArgs e)
        {
            Instance instance = ((DataTimer)source).InstanceData;
            NodeCache nc = NodeCache.Instance;

            if (instance.Successor != null)
            {
                Console.WriteLine($"Successor Node Timeout {instance.Successor.ID}");
                nc.DeleteNode(instance.Successor.ID);
                instance.Successor = null;
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
                    predecessorTimer?.Stop();
                }
                else if (value != null && 
                    (this.m_Predecessor == null || this.m_Predecessor.ID != value.ID)) 
                {
                    Log("SetPredeccessor", $"New Predecessor {value}.");
                    this.m_Predecessor = value;
                }
            }
        }
        private void StartPredecessorPropertyTimer()
        {
            predecessorTimer?.Stop();
            predecessorTimer = new DataTimer();
            predecessorTimer.InstanceData = this;
            predecessorTimer.Elapsed += new ElapsedEventHandler(OnPredecessorTimerEvent);
            predecessorTimer.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["StabilizePredecessorsPeriod"]) * 3;
            predecessorTimer.Enabled = true;
        }

        private static void OnPredecessorTimerEvent(object source, ElapsedEventArgs e)
        {
            Instance instance = ((DataTimer)source).InstanceData;
            NodeCache nc = NodeCache.Instance;

            if (instance.Predecessor != null)
            {
                Console.WriteLine($"Predecessor Node Timeout {instance.Predecessor.ID}");
                nc.DeleteNode(instance.Predecessor.ID);
                instance.Predecessor = null;
            }
        }

        public FingerTable FingerTable { get; set;  }
    }

    class DataTimer: Timer
    {
        public Instance InstanceData { set; get; }
    }
}
