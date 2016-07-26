using System;

namespace DCache
{
    public partial class Instance
    {
        public void Depart()
        {
            StopMaintenance();

            try
            {
                departing = true;

                //this.Successor.NotifyNewPredecessor(this.Predecessor);
                //this.Predecessor.NotifyNewSuccessor(this.Successor);
            }
            catch (Exception e)
            {
                //Server.Log(LogLevel.Error, "Navigation", $"Error on Depart ({e.Message}).");
            }
            finally
            {
                // set this node out of the ring
                this.Successor = LocalNode;
                this.Predecessor = LocalNode;
            }
        }
    }
}
