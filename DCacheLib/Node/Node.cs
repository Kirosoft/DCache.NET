using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DCacheLib
{
    public abstract class Node : INode
    {
        public int PortNumber { get; set; }
        public UInt64 ID => GetHash(this.Host.ToUpper() + this.PortNumber.ToString());
        public string Host {get; set; }
        public Socket SocketRef { get; set; }
        public string Message { get; private set; }

        public delegate string CommandListener(string command, string value);
        public delegate string ResponseListener(string command, string value);
        // Event to indicate a command needs to be processed
        public CommandListener commandListener = null;
        // Event to indicate we have recevied a response to a previously sent command
        public ResponseListener responseListener = null;
        private UdpClient udpClient;
        private Thread receiveThread;
        private UdpClient udpSendClient;
        private IPEndPoint multicastEndPoint;

        public Node(int portNum, string host = API.LOCAL_HOST)
        {
            PortNumber = portNum;
            Host = host;

            udpSendClient = new UdpClient(AddressFamily.InterNetwork);
            var address = IPAddress.Parse(ConfigurationManager.AppSettings["MulticastGroup"]);
            multicastEndPoint = new IPEndPoint(address, Convert.ToInt32(ConfigurationManager.AppSettings["MulticastPort"]));
            udpSendClient.JoinMulticastGroup(address);

        }

        public int CompareTo(object obj)
        {
            if (obj is Node)
                return ID.CompareTo(((Node)obj).ID);

            throw new ArgumentException("Object is not a Node.");
        }

        public override bool Equals(object obj)
        {
            try
            {
                Node node = (Node)obj;
                return ID == node.ID;
            }
            catch(Exception ee)
            {
                Console.WriteLine($"Node:Equals - Error during equals operation ({ee.Message}).");
                return false;
            }
        }

        public virtual bool Send(string data)
        {
            int res = 0;

            if (SocketRef != null)
            {
                res = SocketRef.Send(Encoding.UTF8.GetBytes(data + "<EOF>"));
                //Console.WriteLine($"Sending: {data}");
                if (res == 0)
                {
                    Console.WriteLine("ERRROR********");
                }
            }
            return res > 0;
        }

        public override string ToString()
        {
            return $"{this.Host}:{this.PortNumber.ToString()} ({this.ID.ToString("x10").ToUpper()})";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void MulticastSend(string message)
        {
            var data = Encoding.Default.GetBytes(message);
            udpSendClient.Send(data, data.Length, multicastEndPoint);
        }

        public void StartMulticast()
        {
            try
            {
                udpClient = new UdpClient();
                udpClient.MulticastLoopback = true;
                udpClient.ExclusiveAddressUse = false;
                IPEndPoint localIP = new IPEndPoint(IPAddress.Any, Convert.ToInt32(ConfigurationManager.AppSettings["MulticastPort"]));

                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.ExclusiveAddressUse = false;
                udpClient.Client.Bind(localIP);
                udpClient.JoinMulticastGroup(IPAddress.Parse(ConfigurationManager.AppSettings["MulticastGroup"]), 50);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
            receiveThread = new Thread(Receive);
            receiveThread.Start();
        }

        public void Receive()
        {
            while (true)
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var data = udpClient.Receive(ref ipEndPoint);

                Message = Encoding.Default.GetString(data);
                //Console.WriteLine($"[{PortNumber}] Multicast Message recevied from: {Message}.");

                string command = Message.Split('=')[0];
                string payload = Message.Split('=')[1];

                commandListener?.Invoke(command, payload);
            }
        }

        public static UInt64 GetHash(string key)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.ASCII.GetBytes(key);
            bytes = md5.ComputeHash(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
