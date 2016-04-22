using System;

namespace DCacheLib
{
    public partial class Instance
    {
        public ServerNode LocalNode { get; set; }
        public string Host => LocalNode.Host;
        public int Port => LocalNode.PortNumber;
        public UInt64 ID => LocalNode.ID;
        
        public Node Successor 
        {
            get 
            { 
                return FingerTable[0]; 
            }
            set
            {
                if (value == null && value != FingerTable[0])
                {
                    Log("Navigation", "Setting successor to null.");
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

        private Node m_Predecessor = null;

        public Node Predecessor
        {
            get 
            { 
                return this.m_Predecessor; 
            }
            set
            {
                if (value == null && value != this.m_Predecessor)
                {
                    Log("Navigation", "Setting predecessor to null.");
                }
                else if (value != null && 
                    (this.m_Predecessor == null || this.m_Predecessor.ID != value.ID))   // (otherwise, no change...)
                {
                    Log("SetPredeccessor", $"New Predecessor {value}.");
                    this.m_Predecessor = value;
                }
            }
        }

        public FingerTable FingerTable { get; set;  }
    }
}
