
namespace DCache.Command
{
    public class API
    {
        public const string LOCAL_HOST = "127.0.0.1";

        // Ring maintenance API
        public const string NOTIFY = "NOTIFY";
        public const string PING = "PING";
        public const string FIND_SUCCESSOR = "FIND_SUCCESSOR";
        public const string FIND_PREDECCESSOR = "FIND_PREDECCESSOR";

        public const string GET_SUCCESSOR_RESPONSE = "GET_SUCCESSOR_RESPONSE";
        public const string GET_PREDECESSOR_RESPONSE = "GET_PREDECESSOR_RESPONSE";


    }
}
