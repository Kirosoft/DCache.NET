using System;
using NChordLib;
using System.Linq;

namespace DCacheServer
{
    public class DCacheServerConsole
    {
        public ChordInstance Join(int portNum = 5000, int seedPort = -1, string seedHost = "127.0.0.1")
        {
            ChordInstance instance = null;
            ChordNode seedNode = null;
            ChordServer.LocalNode = new ChordNode(System.Net.Dns.GetHostName(), portNum);

            if (ChordServer.RegisterService(portNum))
            {
                if (seedPort != -1)
                {
                    seedNode = new ChordNode(seedHost, seedPort);
                }
                instance = ChordServer.GetInstance(ChordServer.LocalNode);
                instance.Join(seedNode, ChordServer.LocalNode.Host, ChordServer.LocalNode.PortNumber);
            }
            return instance;
        }

        public bool CheckConsole(ChordInstance instance)
        {
            char charCode = Char.ToUpperInvariant(Console.ReadKey(true).KeyChar);

            if (charCode == 'Q')
                return false;
            else if (charCode == 'I' || charCode == 'X')
                PrintNodeInfo(instance, charCode == 'I' ? false : true);
            else
                Console.WriteLine("Get Server [I]nfo, E[x]tended Info, [Q]uit, or Get Help[?]");

            return true;
        }

        /// <summary>
        /// Print information about a given Chord node.
        /// </summary>
        /// <param name="instance">The Chord instance to get information from.</param>
        /// <param name="extended">Whether or not to print extended information.</param>
        public void PrintNodeInfo(ChordInstance instance, bool extended)
        {
            ChordNode successor = instance.Successor;
            ChordNode predecessor = instance.Predecessor;
            string successorString = successor != null ? successor.ToString() : "NULL";
            string predecessorString = predecessor != null ? successor.ToString() : "NULL";
            Console.WriteLine("\n\rNODE INFORMATION:\n\rSuccessor: {1}\r\nLocal Node: {0}\r\nPredecessor: {2}\r\n", ChordServer.LocalNode, successorString, predecessorString);

            if (extended)
            {
                ChordFingerTable fingerTable = instance.FingerTable;
                ChordNode[] successorCache = instance.SuccessorCache;
                string successorCacheString = "SUCCESSOR CACHE:" + String.Concat(successorCache.Select(s => s != null ? "\n\r" + s.ToString() : "NULL").ToArray());
                string fingerTableString = "FINGER TABLE:";

                for (int i = 0; i < fingerTable.Length; i++)
                {
                    fingerTableString += string.Format("\n\r{0:x8}: ", fingerTable.StartValues[i]);
                    fingerTableString += fingerTable.Successors[i] != null ? fingerTable.Successors[i].ToString() : "NULL";
                }

                Console.WriteLine("\n\r" + successorCacheString);
                Console.WriteLine("\n\r" + fingerTableString);
            }
        }
    }
}
