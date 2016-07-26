using DCache.Command;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DCache.Utils;
using NetSockets;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace DCache
{
    public abstract class Node : INode
    {
        public int PortNumber { get; set; }
        public UInt64 ID => General.GetHash(this.Host.ToUpper() + this.PortNumber.ToString());
        public string Host {get; set; }
        public NetPayloadClient SocketRef { get; set; }
        public string Message { get; private set; }

        public delegate string CommandListener(CommandBuilder command);
        public delegate string ResponseListener(string command, string value);
        // Event to indicate a command needs to be processed
        public CommandListener commandListener = null;
        // Event to indicate we have recevied a response to a previously sent command
        public ResponseListener responseListener = null;
        private UdpClient udpClient;
        private Thread receiveThread;
        private UdpClient udpSendClient;
        private IPEndPoint multicastEndPoint;
        private IConfigurationRoot config;

        public Node(int portNum, string host = API.LOCAL_HOST)
        {
            PortNumber = portNum;
            Host = host;
            config = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json")
                            .Build();

            udpSendClient = new UdpClient(AddressFamily.InterNetwork);
            var address = IPAddress.Parse(config["settings:MulticastGroup"]);
            multicastEndPoint = new IPEndPoint(address, Convert.ToInt32(config["settings:MulticastPort"]));
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

        public virtual string Send(string data)
        {
            int res = -1;
            byte[] ackBuffer = new byte[1024];

            if (SocketRef != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);

                try
                {
                    SocketRef.Send(Encoding.UTF8.GetBytes(data));

                }
                catch (Exception ee)
                {
                    Console.WriteLine($"Socket send error {ee.Message}");
                }
            }
            return Encoding.UTF8.GetString(ackBuffer, 0, ackBuffer.Length);
        }

        public virtual bool SendAsync(string data)
        {
            bool res = false;

            if (SocketRef != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.SetBuffer(buffer, 0, buffer.Length);
                e.Completed += new EventHandler<SocketAsyncEventArgs>(SendAsyncCallback);

                try
                {
                    SocketRef.SendAsync(buffer);
                }
                catch (Exception ee)
                {
                    Console.WriteLine($"Socket send async error {ee.Message}");
                    System.Threading.Tasks.Task.Delay(3000).Wait();
                }
            }
            return true;
        }

        private void SendAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.Count != 0)
            {
                // You may need to specify some type of state and 
                // pass it into the BeginSend method so you don't start
                // sending from scratch
            }
            else
            {
                Console.WriteLine("Socket Error: {0} when sending to {1}",
                       e.SocketError,
                       ID);
            }
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
            var data = Encoding.UTF8.GetBytes(message);
            udpSendClient.SendAsync(data, data.Length, multicastEndPoint);
        }

        public void StartMulticast()
        {
            try
            {
                udpClient = new UdpClient();
                udpClient.MulticastLoopback = true;
                udpClient.ExclusiveAddressUse = false;
                IPEndPoint localIP = new IPEndPoint(IPAddress.Any, Convert.ToInt32(config["settings:MulticastPort"]));

                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.ExclusiveAddressUse = false;
                udpClient.Client.Bind(localIP);
                udpClient.JoinMulticastGroup(IPAddress.Parse(config["settings:MulticastGroup"]), 50);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
            receiveThread = new Thread(MulticastReceive);
            receiveThread.Start();
        }

        public async void MulticastReceive()
        {
            while (true)
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                UdpReceiveResult res = await udpClient.ReceiveAsync();

                Message = Encoding.UTF8.GetString(res.Buffer);
                //Console.WriteLine($"[{PortNumber}] Multicast Message recevied from: {Message}.");
                CommandBuilder command = new CommandBuilder(Message);

                commandListener?.Invoke(command);
            }
        }

        public void Close()
        {
            try
            {
                udpClient?.Dispose();
                SocketRef?.Disconnect();
            }
            catch (Exception ee)
            {
                Console.WriteLine($"Node::Close - Error during cleanup ({ee.Message}).");
            }
        }

    }
}
