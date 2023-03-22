using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDiscoveryLib4
{
    [Serializable]
    internal class DiscoveryPacket
    {
        public int Version { get; set; }
        public string IPv4 { get; set; }
        public string Hostname { get; set; }
        public string Description { get; set; }
    }
}
