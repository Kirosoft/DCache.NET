using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DCache.Cluster
{
    public class NodeCache : ConcurrentDictionary<UInt64, Node>
    {
        private static Semaphore sem = new Semaphore(5, 5);
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

        public Node CheckOrAdd(JObject payload)
        {
            Node result = null;
            sem.WaitOne();

            try
            {
                // notified of a new node, add it to our nodeCache object
                if (!ContainsKey((UInt64)payload["source_node_id"]))
                {
                    result = new ClientNode((int)payload["source_port"], (string)payload["source_host"]);
                    TryAdd((UInt64)payload["source_node_id"], result);
                }
                else
                {
                    result = (Node)this[(UInt64)payload["source_node_id"]];
                }


            }
            finally
            {
                sem.Release(1);
            }

            return result;
        }

        public Node FindNodeForID(UInt64 baseId, UInt64 id)
        {
            // TODO optimise for performance
            List<UInt64> afterList = this.Keys.Where(x => x > baseId).OrderBy(x => x).ToList();
            List<UInt64> beforeList = this.Keys.Where(x => x < baseId).OrderBy(x => x).ToList();
            List<UInt64> orderedList = afterList.Concat(beforeList).ToList();

            UInt64 previousKey = baseId;

            if (orderedList.Count() == 0)
                return null;

            foreach (UInt64 key in orderedList)
            {
                if ((key > previousKey && id > previousKey && id < key) ||
                    (key < previousKey && id > previousKey) || (key < previousKey && id < key))
                {
                    return (Node)this[key];
                }
                previousKey = key;
            }
            return (Node)this[orderedList.Last()];
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
