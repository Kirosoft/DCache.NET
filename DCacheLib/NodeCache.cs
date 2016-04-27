using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;

namespace DCacheLib
{
    public class NodeCache : ConcurrentDictionary<UInt64, Node>
    {
        private static NodeCache instance;
        public static NodeCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NodeCache();
                }
                return instance;
            }
        }

        public Node CheckOrAdd(string value)
        {
            Node result = null;
            JObject payload = JObject.Parse(value);

            // notified of a new node, add it to our nodeCache object
            if (!ContainsKey((UInt64)payload["source_node_id"]))
            {
                result = new ClientNode((int)payload["source_port"], (string)payload["source_host"]);
                TryAdd((UInt64)payload["source_node_id"],result);
            } else
            {
                result = (Node)this[(UInt64)payload["source_node_id"]];
            }

            return result;
        }
        public bool DeleteNode(UInt64 id)
        {
            Node value = null;
            bool result = false;

            result =  this.TryRemove(id, out value);
            if (result)
            {
                value.Close();
            }
            return result;
        }
    }
}
