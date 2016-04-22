using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace DCacheLib
{
    public class LocationService
    {
        private int basePort = Convert.ToInt32(ConfigurationManager.AppSettings["BasePort"]);
        private int maxConnections = Convert.ToInt32(ConfigurationManager.AppSettings["MaxClusterSize"]);
        private static LocationService instance;
        public static LocationService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LocationService();
                }
                return instance;
            }
        }

        public List<int> GetConnectedPorts(int excludePort = -1)
        {
            List<int> connectionList = new List<int>();
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] connections = ipGlobalProperties.GetActiveTcpListeners();
            connectionList = connections.Where(x => x.Port >= basePort && x.Port < (basePort + maxConnections) && x.Port != excludePort)
                                        .Select(x => x.Port)
                                        .ToList<int>();
            return connectionList;
        }

        public int GetNextAvailablePort()
        {
            int basePort = Convert.ToInt32(ConfigurationManager.AppSettings["BasePort"]);

            int[] connectedPorts = GetConnectedPorts().ToArray<int>();
            if (connectedPorts.Length == 0)
            {
                return basePort;
            } 
            for (int f = 0; f < connectedPorts.Length; f++)
            {
                if (connectedPorts[f] != basePort+f)
                {
                    return basePort + f;
                }
            }
            return basePort+connectedPorts.Length;
        }

        // Gets any other cluster node (we can exclude ourselves)
        public ClientNode GetClusterNode(int excludePort = -1)
        {
            ClientNode clientNode = null;

            List<int> portList = GetConnectedPorts(excludePort);
            if (portList.Count > 0)
            {
                clientNode = new ClientNode(portList[0]);
            }

            return clientNode;
        }
    }
}
