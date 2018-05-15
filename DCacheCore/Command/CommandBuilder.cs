using Newtonsoft.Json.Linq;

namespace DCache.Command
{
    public class CommandBuilder
    {
        public string Command { get; set; }
        public JObject Payload { get; set; }
        // Application level API
        public const string PUT_KEYS = "PUT_KEYS";
        public const string GET_KEYS = "GET_KEYS";

        public CommandBuilder()
        {
            Init();            
        }
        public CommandBuilder(string content)
        {
            Init();
            string innerContent = content.Substring(1, content.Length - 2);
            this.Command = innerContent.Split('=')[0];
            this.Payload = JObject.Parse(innerContent.Split('=')[1]);
        }

        public CommandBuilder Init()
        {
            Payload = new JObject();
            Payload["batch"] = new JObject();
            Payload["source_node_id"] = null;
            return this;
        }

        public CommandBuilder AddCommand(string command)
        {
            this.Command = command;
            return this;
        }

        public CommandBuilder AddKey(string key, string value)
        {
            Payload["batch"][key] = value;
            return this;
        }

        public CommandBuilder AddProp(string key, string value)
        {
            Payload[key] = value;
            return this;
        }

        public JToken this[string propName]
        {
            get { return Payload[propName]; }
            set { Payload[propName] = value; }
        }

        public override string ToString()
        {
            return $"[{Command}={Payload.ToString()}]";
        }
    }
}
