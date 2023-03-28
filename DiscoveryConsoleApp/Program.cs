using System;
using Newtonsoft.Json;

namespace DiscoveryConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var pkt = new DiscoveryPacket()
            {
                Version = 1,
                Hostname = "MY-PC",
                IPv4 = "192.168.1.99",
                Description = ".NET Framework 4.7.2"
            };

            var jsonStr = JsonConvert.SerializeObject(pkt);

            Console.WriteLine(jsonStr);

            Console.Write("\nPress Enter to continue");
            Console.ReadLine();
        }
    }
}
