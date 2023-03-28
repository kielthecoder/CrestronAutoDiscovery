using System;
using Newtonsoft.Json;

namespace DiscoveryConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var agent = new DiscoveryAgent();
            
            agent.Hostname = "KIEL-PC";
            agent.IPv4 = "192.168.1.99";
            agent.Description = ".NET Framework 4.7.2";

            agent.Start("239.64.64.64", 5555);

            Console.WriteLine("\nPress Enter to exit");
            Console.ReadLine();

            agent.Stop();
        }
    }
}
