using Newtonsoft.Json.Linq;
using System;

namespace DCacheLib
{

    // in a circle of m nodes. Key space is = 0 - 2^m -1
    // key k is assigned to the first node with id >= k (successor node)
    public partial class Instance 
    {

        public Instance(INode hostNode)
        {
            LocalNode = (ServerNode)hostNode;
            LocalNode.commandListener += CommandProcessor;
            FingerTable = new FingerTable((ServerNode)hostNode);

            Log("Instance", $"Starting new node on port {LocalNode.PortNumber}.");

            StartMaintenance();
        }

        public string CommandProcessor(string command, string value)
        {
            JObject payload = JObject.Parse(value);
            UInt64 sourceNodeId = (UInt64)payload["source_node_id"];

            if (sourceNodeId == ID)
            {
                return "";
            }
            NodeCache nc = NodeCache.Instance;
            Node sourceNode = nc.CheckOrAdd(value);
            //("Command Processor", $"{command} with {value}.");
            switch (command)
            {
                case API.NOTIFY:
                    {
                        // just added to the cache
                        break;
                    }
                case API.PING:
                    {
                        // just added to the cache
                        Console.Write($"*");
                        break;
                    }
                case API.FIND_SUCCESSOR:
                    {
                        if (IsIDInRange(sourceNodeId))
                        {
                            // THe property will fire an event
                            Successor = sourceNode;
                        }
                        break;
                    }
                case API.FIND_PREDECCESSOR:
                    {
                        // if our SuccessorNode.ID is the same as the requester node id then
                        // we are the predecessor
                        if (Successor == sourceNode)
                        {
                            sourceNode.Send($"{API.GET_PREDECESSOR_RESPONSE}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}");
                        }

                        break;
                    }
                case API.GET_SUCCESSOR_RESPONSE:
                    {
                        // A node has responded and believes it is the successor
                        Successor = sourceNode;
                        break;
                    }
                case API.GET_PREDECESSOR_RESPONSE:
                    {
                        // A node has responded and believes it is the predecessor
                        Predecessor = sourceNode;
                        break;
                    }
                default:
                    Log("command processor",$"Unknown command recevied {command} with {value}.");
                    break;
            }

            return "";
        }

        public bool IsIDInRange(UInt64 id)
        {
            UInt64 end = Successor != null ? Successor.ID : ID;

            if (ID >= end)
            {
                if (id > ID || id <= end)
                {
                    return true;
                }
            }
            else
            {
                if (id > ID && id <= end)
                {
                    return true;
                }
            }
            return false;
        }

        public string ToString(bool extended = false)
        {
            string result = "";
            string successorString = Successor != null ? Successor.ToString() : "NULL";
            string predecessorString = Predecessor != null ? Predecessor.ToString() : "NULL";
            result += $"\n\rNODE INFORMATION:\n\rLocal Node: {LocalNode}\r\nPredecessor: {predecessorString}\r\nSuccessor: {successorString}\r\n";
            if (extended)
            {
                result += "\n\r" + FingerTable.ToString();
            }
            return result;
        }

        public void Log(string logArea, string message, LogLevel logLevel = LogLevel.Debug, params object[] parameters)
        {
            Console.WriteLine($"{DateTime.Now} {LocalNode.Host}:{LocalNode.PortNumber} ({LocalNode.ID}) > : { message}");
        }

        public enum LogLevel
        {
            Error,
            Info,
            Warn,
            Debug
        }

    }
}
