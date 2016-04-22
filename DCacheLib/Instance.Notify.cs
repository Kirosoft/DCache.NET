/*
 * ChordInstance.Notify.cs:
 * 
 *  Implementation of the Notify method used by Chord to notify a node's successor
 *  as to who its predecessor is.  The Notify method may be called in an even safer
 *  fashion via the static CallNotify method in ChordServer for remote calls.
 *
 */

using System;

namespace DCacheLib
{
    public partial class Instance 
    {
        /// <summary>
        /// Called by the predecessor to a remote node, this acts as a dual heartbeat mechanism and more importantly
        /// notification mechanism between predecessor and successor.
        /// </summary>
        /// <param name="node">A ChordNode instance indicating who the calling node (predecessor) is.</param>
        public void Notify(Node node)
        {
            // if the node has absolutely no predecessor, take
            // the first one it finds
            if (this.Predecessor == null)
            {
                this.Predecessor = node;
                return;
            }

            // otherwise, ensure that the predecessor that is calling in
            // is indeed valid...
            //if (IsIDInRange(node.ID, this.Predecessor.ID, this.ID))
            //{
            //    this.Predecessor = node;
            //    return;
            //}
        }
    }
}
