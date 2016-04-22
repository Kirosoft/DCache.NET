using System;
using System.Configuration;

namespace DCacheLib
{
    public partial class Instance
    {
        public bool Join(Node anotherNode)
        {
            // Initialise a default Finger table
            FingerTable = new FingerTable(LocalNode);

            // Position this node correctly in the ring
            // Other nodes in the ring adapt their pointer as part of a background process
            
            //Successor = anotherNode.FindSuccessor(ID);
            //Predecessor = Successor.GetPredecessor();

            // everything that needs to be populated or kept up-to-date
            // lazily is handled via background maintenance threads running periodically
            StartMaintenance();

            return true;
        }
    }
}
