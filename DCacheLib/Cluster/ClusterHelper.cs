using System.Collections.Generic;

namespace DCacheLib.Cluster
{
    public class ClusterHelper
    {
        private static ClusterHelper instance;

        public static ClusterHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ClusterHelper();
                }
                return instance;
            }
        }

        public List<Instance> CreateLocalCluster(int clusterSize)
        {
            List<Instance> cluster = new List<Instance>();
            LocationService locationService = new LocationService();

            for (int f = 0; f < clusterSize; f++)
            {
                Instance server = new Instance(new ServerNode(locationService.GetNextAvailablePort()));
                cluster.Add(server);
            }

            return cluster;
        }
    }
}
