using System;
using System.Collections.Concurrent;

namespace DCacheLib.Services
{
    class MapService : IService
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> partitions = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        string IService.get(string key, string partitionId)
        {
            if (partitionId == "")
            {
                partitionId = key;
            }
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

        string IService.put(string key, string value, string partitionId)
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
    }
}
