using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace AutoDiscoveryLib
{
    class DiscoveryPacket
    {
        public string ID { get; set; }
        public string Address { get; set; }
        public uint Clock { get; set; }

        public DiscoveryPacket()
        {
        }
    }
}