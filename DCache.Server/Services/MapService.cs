using System;
using System.Collections.Concurrent;

namespace DCache.Services
{
    class MapService : IService
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> partitions = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        private UInt64 startID;
        private UInt64? endID;

        public MapService(UInt64 startID)
        {
            this.startID = startID;
        }

        public void SuccessorEvent(UInt64? successorID)
        {
            this.endID = successorID;
        }

        string IService.GetLocal(string key, string partitionId)
        {
            if (partitionId == "")
            {
                partitionId = key;
            }
            try
            {
                var partition = partitions[partitionId];

                if (partition != null)
                {
                    return partition[key];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
            return null;
        }

        string IService.PutLocal(string key, string value, string partitionId)
        {
            if (partitionId == "")
            {
                partitionId = key;
            }
            if (!partitions.ContainsKey(partitionId))
            {
                partitions[partitionId] = new ConcurrentDictionary<string, string>();
            }
            return partitions[partitionId].AddOrUpdate(key, value, (akey, oldValue) => value);
        }

        public override string ToString()
        {
            string result = "";

            foreach (string partitionId in partitions.Keys)
            {
                if (partitions[partitionId] != null)
                {
                    result += ($"Partition ID: {partitionId}\n");
                    result += ($"---------------------------\n");

                    foreach(string key in partitions[partitionId].Keys)
                    {
                        result += $"{key}:{partitions[partitionId][key]}\n";
                    }
                    result += "\n";
                }
            }

            return result;
        }
    }
}
