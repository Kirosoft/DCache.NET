/*
 * ChordServer.cs:
 * 
 *  ChordServer is a kitchen sink of static methods and properties for use in:
 * 
 *      * Safely interacting with the Chord DHT (simplifying retry & exception handling) locally and remotely.
 *      * Getting a raw ChordInstance remoting instance (do your own exception handling / validation).
 *      * Performing "Chord-math" for doing wraparound comparisons on IDs and finger table entries.
 *      * Logging to a common logging facility (used by client and server code alike).
 *      * Common remoting service registration / un-regstration.
 *
 */

using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace DCacheLib
{
    /// <summary>
    /// Static methods and properties for various Chord functionality.
    /// </summary>
    public static class Server
    {
        /// <summary>
        /// The local ChordNode identification.  Used in logging (to log to the correct log file) and navigation.
        /// </summary>
        public static Node LocalNode { get; set; }
        public static int MaxRetries => Convert.ToInt32(ConfigurationManager.AppSettings["Retries"]);

        #region Safe Remote Method / Property Access

        /*
         * Retry logic:
         *  The idea behind the retry logic is to provide a simple and common-case reusable call
         *  in to remote methods or properties.  This logic also serendipitously encapsulates and
         *  simplifies exception handling by performing a bounded number of retries as part of
         *  exception handling.  The retryCount that is passed along as part of the retry logic
         *  serves as a pleasant way to maintain state across node boundaries (thus enforcing a
         *  fixed number of N retries for a logical operation, no matter how many nodes the
         *  operation spans.
         * 
         *  Currently, the default retry count is hardcoded; in the future it may be desirable to
         *  expose this value as a configurable parameter.
         * 
         * Safe access & exception handling pattern:
         *  Anywhere client or server code needs to make remoting calls, there are typically two 
         *  things people usually do: wrap the call in some sort of exception handling (not doing
         *  this is generally silly - and is a quick way to wreck whatever application is consuming
         *  that code upstream), and peform a fixed number of retries in case of transient errors
         *  (transient errors can be somewhat common when testing with many hundreds of Chord nodes
         *  running simultaneously on a single OS instance - often, retrying fatal-seeming errors
         *  can lead to success, reducing the need to exercise (harsher) upstream failure handling).
         *  
         *  In almost all cases, upstream code patterns performing these remote access / invocations
         *  use a single exception handling path; therefore, error is signaled simply via return value
         *  for simple error-handling (since retry is not needed).
         *  
         */


        /// <summary>
        /// Calls Notify() remotely, using a default retry value of three.
        /// </summary>
        /// <param name="remoteNode">The remote on which to call the method.</param>
        /// <param name="callingNode">The node to inform the remoteNode of.</param>
        /// <returns>True if succeeded, FALSE otherwise.</returns>
        public static bool CallNotify(Node remoteNode, Node callingNode)
        {
            return CallNotify(remoteNode, callingNode, MaxRetries);
        }

        /// <summary>
        /// Calls Notify() remotely, using a default retry value of three.
        /// </summary>
        /// <param name="remoteNode">The remote on which to call the method.</param>
        /// <param name="callingNode">The node to inform the remoteNode of.</param>
        /// <param name="retryCount">The number of times to retry the operation in case of error.</param>
        /// <returns>True if succeeded, FALSE otherwise.</returns>
        public static bool CallNotify(Node remoteNode, Node callingNode, int retryCount)
        {
            try
            {
                remoteNode.Notify(callingNode);
                return true;
            }
            catch (Exception ex)
            {
                //Server.Log(LogLevel.Debug, "Remote Invoker", "CallNotify error: {0}", ex.Message);

                if (retryCount > 0)
                {
                    return CallNotify(remoteNode, callingNode, --retryCount);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Calls FindSuccessor() remotely, using a default retry value of three.  HopCount is ignored.
        /// </summary>
        /// <param name="remoteNode">The remote on which to call the method.</param>
        /// <param name="id">The ID to look up.</param>
        /// <returns>The Successor of ID, or NULL in case of error.</returns>
        public static Node CallFindSuccessor(Node remoteNode, UInt64 id)
        {
            int hopCountOut = 0;
            return CallFindSuccessor(remoteNode, id, MaxRetries, 0, out hopCountOut);
        }

        /// <summary>
        /// Convenience function to call FindSuccessor using ChordServer.LocalNode as the
        /// "remote" node.
        /// </summary>
        /// <param name="id"> The ID to look up (ChordServer.LocalNode is used as the remoteNode).</param>
        /// <returns>The Successor of ID, or NULL in case of error.</returns>
        public static Node CallFindSuccessor(UInt64 id)
        {
            return CallFindSuccessor(Server.LocalNode, id);
        }

        /// <summary>
        /// Calls FindSuccessor() remotely, using a default retry value of three.
        /// </summary>
        /// <param name="remoteNode">The remote node on which to call FindSuccessor().</param>
        /// <param name="id">The ID to look up.</param>
        /// <param name="retryCount">The number of times to retry the operation in case of error.</param>
        /// <param name="hopCountIn">The known hopcount prior to calling FindSuccessor on this node.</param>
        /// <param name="hopCountOut">The total hopcount of this operation (either returned upwards, or reported for hopcount efficiency validation).</param>
        /// <returns>The Successor of ID, or NULL in case of error.</returns>
        public static Node CallFindSuccessor(Node remoteNode, UInt64 id, int retryCount, int hopCountIn, out int hopCountOut)
        {

            try
            {
                return remoteNode.FindSuccessor(id, hopCountIn, out hopCountOut);
            }
            catch (Exception ex)
            {
                //Server.Log(LogLevel.Debug, "Remote Invoker", "CallFindSuccessor error: {0}", ex.Message);

                if (retryCount > 0)
                {
                    return CallFindSuccessor(remoteNode, id, --retryCount, hopCountIn, out hopCountOut);
                }
                else
                {
                    hopCountOut = hopCountIn;
                    return null;
                }
            }
        }

       
        /// <summary>
        /// Convenience function to get the local node's Predecessor.
        /// </summary>
        /// <returns>The Predecessor of ChordServer.LocalNode, or NULL in case of error.</returns>
        public static Node GetPredecessor()
        {
            return GetPredecessor(Server.LocalNode);
        }

        /// <summary>
        /// Gets the remote Predecessor property, using a default retry value of three.
        /// </summary>
        /// <param name="remoteNode">The remote from which to access the property.</param>
        /// <returns>The remote node's predecessor, or NULL in case of error.</returns>
        public static Node GetPredecessor(Node remoteNode)
        {
            return GetPredecessor(remoteNode, MaxRetries);
        }

        /// <summary>
        /// Gets the remote Predecessor property, given a custom retry count.
        /// </summary>
        /// <param name="remoteNode">The remote node from which to access the property.</param>
        /// <param name="retryCount">The number of times to retry the operation in case of error.</param>
        /// <returns>The remote predecessor, or NULL in case of error.</returns>
        public static Node GetPredecessor(Node remoteNode, int retryCount)
        {
            try
            {
                return remoteNode.GetPredecessor();
            }
            catch (System.Exception ex)
            {
                //Server.Log(LogLevel.Debug, "Remote Accessor", "GetPredecessor error: {0}", ex.Message);

                if (retryCount > 0)
                {
                    return GetPredecessor(remoteNode, --retryCount);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Convenience function to retrieve the local node's Successor property.
        /// </summary>
        /// <returns>The local node's successor, or NULL in case of error.</returns>
        public static Node GetSuccessor()
        {
            return GetSuccessor(Server.LocalNode);
        }

        /// <summary>
        /// Gets the remote Successor property, using a default retry value of three.
        /// </summary>
        /// <param name="remoteNode">The remote from which to access the property.</param>
        /// <returns>The remote node's successor, or NULL in case of error.</returns>
        public static Node GetSuccessor(Node remoteNode)
        {
            return GetSuccessor(remoteNode, MaxRetries);
        }

        /// <summary>
        /// Gets the remote Successor property, given a custom retry count.
        /// </summary>
        /// <param name="remoteNode">The remote node from which to access the property.</param>
        /// <param name="retryCount">The number of times to retry the operation in case of error.</param>
        /// <returns>The remote successor, or NULL in case of error.</returns>
        public static Node GetSuccessor(Node remoteNode, int retryCount)
        {
            try
            {
                return remoteNode.GetSuccessor();
            }
            catch (System.Exception ex)
            {
                //Server.Log(LogLevel.Debug, "Remote Accessor", "GetSuccessor error: {0}", ex.Message);

                if (retryCount > 0)
                {
                    return GetSuccessor(remoteNode, --retryCount);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Safely checks whether a ChordInstance is valid by ensuring the port and successor values are valid.
        /// </summary>
        /// <param name="instance">The ChordInstance to validity-check.</param>
        /// <returns>TRUE if valid; FALSE otherwise.</returns>
        public static bool IsInstanceValid(Instance instance)
        {
            try
            {
                if (instance.Port > 0 && instance.Successor != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                //Log(LogLevel.Debug, "Incoming instance was not valid: ({0}).", e.ToString());  // TODO; better logging
                return false;
            }
        }

        #endregion

        #region Chord-math Functionality

        /// <summary>
        /// Checks whether a key is in a specified range.  Handles wraparound for cases where the start value is
        /// bigger than the end value.  Used extensively as a convenience function to determine whether or not a
        /// piece of data belongs in a given location.
        /// 
        /// Most typically, IsIDInRange is used to determine whether a key is between the local ID and the successor ID:
        ///     IsIDInRange(key, this.ID, this.Successor.ID);
        /// </summary>
        /// <param name="id">The ID to range-check.</param>
        /// <param name="start">The "low" end of the range.</param>
        /// <param name="end">The "high" end of the range.</param>
        /// <returns>TRUE if ID is in range; FALSE otherwise.</returns>
        public static bool IsIDInRange(UInt64 id, UInt64 start, UInt64 end)
        {
            if (start >= end)
            {
                // this handles the wraparound and single-node case.  for wraparound, the range includes zero, so any key
                // that is bigger than start or smaller than or equal to end is in the range.  for single-node, our nodehash
                // will equal the successor nodehash (we are our own successor), and there's no way a key can't fall in the range
                // because if range == X, then key must be either >, < or == X which will always happen!
                if (id > start || id <= end)
                {
                    return true;
                }
            }
            else
            {
                // this is the normal case where we want the key to fall between the lower bound of start and the upper bound of end
                if (id > start && id <= end)
                {
                    return true;
                }
            }
            // for all other cases we're not in range
            return false;
        }

        public static bool FingerInRange(UInt64 key, UInt64 start, UInt64 end)
        {
            if (start == end)
            {
                return true;
            }
            else if (start > end)
            {
                if (key > start || key < end)
                {
                    return true;
                }
            }
            else
            {
                if (key > start && key < end)
                {
                    return true;
                }
            }
            // for all other cases, we're not in the range
            return false;
        }

        #endregion


        /// <summary>
        /// Gets the 64-bit truncated MD5 hash value of a given string key.
        /// </summary>
        /// <param name="key">The key to hash.</param>
        /// <returns>A ulong-truncated MD5 hash digest of the string key.</returns>
        public static UInt64 GetHash(string key)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.ASCII.GetBytes(key);
            bytes = md5.ComputeHash(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }

    

}
