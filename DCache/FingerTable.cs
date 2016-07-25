using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DCache
{
    public class FingerTable
    {
        private ServerNode LocalNode { set; get; }
        public UInt64[] Start { get; set; }
        public Node[] Fingers = null;
        public int Length => Fingers.Length;
        public UInt64 ID { set; get; }
        private IConfigurationRoot config;

        public FingerTable(ServerNode localNode)
        {
            this.LocalNode = localNode;
            config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

            int tableSize = Convert.ToInt32(config["settings:FingerTableSize"]);

            Start = new UInt64[tableSize];
            Fingers = new Node[tableSize];

            // populate the start array and successors
            for (int i = 0; i < Length; i++)
            {
                this.Start[i] = (localNode.ID + (UInt64)Math.Pow(2, i)) % UInt64.MaxValue;
                Fingers[i] = null;
            }

        }

        public Node this[int i]
        {
            get { return Fingers[i]; }
            set { Fingers[i] = value; }
        }

        public Node FindClosestSuccessor(UInt64 id)
        {
            Node closest = null;

            for (int i = Start.Length - 1; i >= 0; i--)
            {
                
            }

            return closest;
        }

        public Node FindClosestPrecedingFinger(UInt64 targetId)
        {
            for (int i = Fingers.Length - 1; i >= 0; i--)
            {
                if (Fingers[i] != null && Fingers[i] != LocalNode)
                {
                    if (FingerInRange(Fingers[i].ID, ID, targetId))
                    {
                        return Fingers[i];
                    }
                }
            }

            return LocalNode;
        }

        public bool FingerInRange(UInt64 key, UInt64 start, UInt64 end)
        {
            if (start == end)
            {
                return true;
            }
            else if (start > end)
            {
                if (key > start || key < end)
                {
                    return true;
                }
            }
            else
            {
                if (key > start && key < end)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            string fingerTableString = "FINGER TABLE:";

            for (int i = 0; i < Fingers.Length; i++)
            {
                fingerTableString += string.Format("\n\r{0:x8}: ", Start[i]);
                fingerTableString += Fingers[i] != null ? Fingers[i].ToString() : "NULL";
            }

            return fingerTableString;
        }
    }
}
