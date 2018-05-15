using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DCache.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            DCacheServerConsole dconsole = new DCacheServerConsole();

            try
            {
                int portNum = args.Length >= 1 ? Convert.ToInt32(args[0]) : 5000;
                int seedPort = args.Length >= 2 ? Convert.ToInt32(args[1]) : -1;
                string seedHost = args.Length >= 3 ? Convert.ToString(args[2]) : "127.0.0.1";

                List<Instance> cluster = dconsole.Join(portNum, 1);

                if (cluster != null && cluster.Count > 0)
                {
                    while (dconsole.CheckConsole(cluster)) ;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception: {0}", ex);
                Usage();
            }
        }

        /// <summary>
        /// Print usage information.
        /// </summary>
        public static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tDCacheServer.exe [portToRunOn] [ <seedPort> ] [ <seedHost> ]");
            Console.WriteLine("\tPortToRunOn will default to 5000 if omitted.");
            Console.WriteLine("\tseedHost will default 127.0.0.1 if omitted.");
        }
    }
}

