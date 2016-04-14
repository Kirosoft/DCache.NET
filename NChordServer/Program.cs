using NChordLib;
using System;

namespace DCacheServer
{
    class Program
    {
        static void Main(string[] args)
        {
            DCacheServerConsole dconsole = new DCacheServerConsole();

            try
            {
                int portNum = args.Length >= 1 ? Convert.ToInt32(args[0]) : 5000;
                int seedPort = args.Length >= 2 ? Convert.ToInt32(args[1]) : -1;
                string seedHost = args.Length >= 3 ? Convert.ToString(args[2]) : "127.0.0.1";

                ChordInstance instance = dconsole.Join(portNum, seedPort, seedHost);
                
                if (instance != null)
                {
                    while (dconsole.CheckConsole(instance)) ;
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
