using System;
using DCacheLib;
using System.Collections.Generic;
using System.Linq;
using DCacheLib.Cluster;

namespace DCacheServer
{
    public class DCacheServerConsole
    {
        public List<Instance> Join(int portNum = 5000, int clusterSize = 1)
        {
            ClusterHelper clusterHelper = ClusterHelper.Instance;

            List<Instance> cluster = clusterHelper.CreateLocalCluster(clusterSize);

            LocationService locationService = new LocationService();
            List<int> clusterList = locationService.GetConnectedPorts();
            Console.WriteLine("LocationServices: "+String.Join(",", clusterList.Select(x => x.ToString()).ToArray()));

            return cluster;
        }

        public bool CheckConsole(List<Instance> instanceList)
        {
            char charCode = Char.ToUpperInvariant(Console.ReadKey(true).KeyChar);

            if (charCode == 'Q')
                return false;
            else if (charCode == 'I' || charCode == 'X')
            {
                foreach (Instance instance in instanceList)
                {
                    Console.WriteLine(instance.ToString(charCode == 'I' ? false : true));
                }
            }
            else
                Console.WriteLine("Get Server [I]nfo, E[x]tended Info, [Q]uit, or Get Help[?]");

            return true;
        }
      
    }
}
