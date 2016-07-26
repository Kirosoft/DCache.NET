using System;
using System.Collections.Concurrent;

namespace DCache.Services
{
    public class ServiceManager
    {
        private ConcurrentDictionary<string, IService> services = new ConcurrentDictionary<string, IService>();

        public ServiceManager(Instance instance)
        {
            MapService mapServer = new MapService(instance.ID);
            services.TryAdd("_system", new MapService(instance.ID));
            // Register this service for successor events
            instance.successorEventListener += mapServer.SuccessorEvent;

            services["_system"].PutLocal("start_time", DateTime.Now.ToUniversalTime().ToString());

            Console.WriteLine($"Startup Time: {services["_system"].GetLocal("start_time")}");
        }

        public IService GetService(string serviceName)
        {
            return services[serviceName];
        }

        public override string ToString()
        {
            string result = "\n";

            foreach (IService service in services.Values)
            {
                result += service.ToString();
            }
            return result;
        }
    }
}
