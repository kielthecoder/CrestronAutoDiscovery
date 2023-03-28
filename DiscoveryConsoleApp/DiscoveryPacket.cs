using System;

namespace DiscoveryConsoleApp
{
    internal class DiscoveryPacket
    {
        public int Version { get; set; }
        public string IPv4 { get; set; }
        public string Hostname { get; set; }
        public string Description { get; set; }
    }
}
