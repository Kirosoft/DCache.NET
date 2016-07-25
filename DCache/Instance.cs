using Newtonsoft.Json.Linq;
using System;
using DCache.Services;
using DCache.Command;
using DCache.Utils;
using Microsoft.Extensions.Configuration;
using System.IO;

//TODO: workout backup strategy
// if id in range store in this node and wait for backup node response
// if id not in range store in this node (as a backup) and wait for confirmation of storage in the remote node
// If a node is removed from the ring then check if any backup was in the list managed by that node else keep the backup
// and duplicate to the new node managing those keys 
// --- How to know if a node is no longer available

// TODO: Create the routing service (fingerTable)

// TODO: Batch key splitting
// Use the affinity id's sort into seperate batch requests
namespace DCache
{

    // in a circle of m nodes. Key space is = 0 - 2^m -1
    // key k is assigned to the first node with id >= k (successor node)
    public partial class Instance 
    {
        private NodeCache nodeCache = NodeCache.Instance;
        private ServiceManager serviceManager;
        private IConfigurationRoot config;

        public Instance(INode hostNode)
        {
            LocalNode = (ServerNode)hostNode;
            LocalNode.commandListener += CommandProcessor;
            FingerTable = new FingerTable((ServerNode)hostNode);

            config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            Log("Instance", $"Starting new node on port {LocalNode.PortNumber}.");

            StartMaintenance();

            serviceManager = new ServiceManager(this);
        }

        public string CommandProcessor(CommandBuilder command)
        {
            UInt64? sourceNodeId = (UInt64?) command.Payload["source_node_id"];

            if (sourceNodeId == ID)
            {
                return "";
            } 

            switch (command.Command)
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
                        if (sourceNodeId != null && IsIDInRange((UInt64) sourceNodeId))
                        {
                            // THe property will fire an event
                            Node sourceNode = nodeCache.CheckOrAdd(command.Payload);
                            Successor = sourceNode;
                        }
                        break;
                    }
                case API.FIND_PREDECCESSOR:
                    {
                        // if our SuccessorNode.ID is the same as the requester node id then
                        // we are the predecessor
                        Node sourceNode = nodeCache.CheckOrAdd(command.Payload);
                        if (sourceNodeId != null && Successor == sourceNode)
                        {
                            sourceNode.SendAsync($"[{API.GET_PREDECESSOR_RESPONSE}={{'source_node_id':'{ID}','source_port':'{Convert.ToInt32(LocalNode.PortNumber)}','source_host':'{LocalNode.Host}'}}]");
                        }

                        break;
                    }
                case API.GET_SUCCESSOR_RESPONSE:
                    {
                        // A node has responded and believes it is the successor
                        Node sourceNode = nodeCache.CheckOrAdd(command.Payload);
                        if (sourceNode != null)
                        {
                            Successor = sourceNode;
                        }
                        break;
                    }
                case API.GET_PREDECESSOR_RESPONSE:
                    {
                        // A node has responded and believes it is the predecessor
                        Node sourceNode = nodeCache.CheckOrAdd(command.Payload);
                        if (sourceNode != null)
                        {
                            Predecessor = sourceNode;
                        }
                        break;
                    }
                case CommandBuilder.PUT_KEYS:
                    {
                        JToken keyData = command.Payload["batch"];
                        foreach(JProperty keypair in keyData)
                        {
                            JObject data = JObject.Parse((string)keypair.Value);
                            string mapName = (string) data.GetValue("map_name");
                            string value = (string)data.GetValue("data");
                            IService service = serviceManager.GetService(mapName);
                            UInt64 keyHash = General.GetHash(keypair.Name);
                            if (IsIDInRange(keyHash))
                            {
                                Console.WriteLine($"Storing key in this node");
                                service.PutLocal(keypair.Name, value);
                            }
                            else
                            {
                                Node remoteNode = nodeCache.FindNodeForID(ID, keyHash);

                                if (remoteNode != null)
                                {
                                    Console.WriteLine($"Re-target to remote: {keypair.Name} with port id: {remoteNode.PortNumber}");
                                    JObject remoteData = new JObject();
                                    remoteData.Add("remote-address", remoteNode.Host);
                                    remoteData.Add("remote-port", remoteNode.PortNumber);
                                    remoteData.Add("remote-id", remoteNode.ID);
                                    return remoteData.ToString();

                                } 
                                else
                                {
                                    Console.WriteLine("Non-local but no remote found " + keypair.Name);
                                    JObject remoteData = new JObject();
                                    remoteData.Add("remote-address", "unknown");
                                    remoteData.Add("remote-port", -1);
                                    remoteData.Add("remote-id", "unknown");
                                    return remoteData.ToString();
                                }

                            }
                            Console.WriteLine("keypair: " + keypair.ToString());   
                        }
                        return "OK";
                    }
                case CommandBuilder.GET_KEYS:
                    {
                        JToken batchData = command.Payload["batch"];
                        foreach (JProperty keypair in batchData.DeepClone())
                        {
                            JObject data = JObject.Parse((string)keypair.Value);
                            string mapName = (string)data.GetValue("map_name");
                            IService service = serviceManager.GetService(mapName);
                            UInt64 keyHash = General.GetHash(keypair.Name);

                            if (IsIDInRange(keyHash))
                            {

                                string keyData = service.GetLocal(keypair.Name);
                                data["data"] = keyData;
                                batchData[keypair.Name] = data.ToString();
                                Console.WriteLine("keypair: " + keypair.ToString());
                            }
                            else
                            {
                                Node remoteNode = nodeCache.FindNodeForID(ID, keyHash);

                                if (remoteNode != null)
                                {
                                    Console.WriteLine("getting key from remote: " + keypair.Name);
                                    //CommandBuilder remoteCommand = new CommandBuilder()
                                    //                            .AddCommand(CommandBuilder.GET_KEYS)
                                    //                            .AddKey(keypair.Name, "{'map_name':'_system','partition_id':'null'}");
                                    //string remotePayload =  remoteNode.Send(remoteCommand.ToString());
                                    //batchData[keypair.Name] = remotePayload;
                                    JObject remoteData = new JObject();
                                    remoteData.Add("address", remoteNode.Host);
                                    remoteData.Add("port", remoteNode.PortNumber);
                                    batchData["remote"] = remoteData.ToString();
                                }

                            }
                        }
                        return batchData.ToString();
                    }
                default:
                    Log("command processor",$"Unknown command recevied {command.Command} with {command.Payload.ToString()}.");
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
                //result += "\n\r" + FingerTable.ToString();

                result += serviceManager.ToString();
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
