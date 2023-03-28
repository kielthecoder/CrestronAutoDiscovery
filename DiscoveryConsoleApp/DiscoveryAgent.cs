using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscoveryConsoleApp
{
    internal struct SocketStatus
    {
        UdpClient Client;

    }

    class DiscoveryAgent
    {
        private IPEndPoint _ep;
        private UdpClient _sock;
        private Thread _poll;

        public bool Active { get; private set; }

        public string Hostname { get; set; }
        public string IPv4 { get; set; }
        public string Description { get; set; }
        public int PollingPeriod { get; set; }

        public event EventHandler OnStarted;
        public event EventHandler OnStopping;

        public DiscoveryAgent()
        {
            // Create a new UDP socket with defaults
            _sock = new UdpClient();

            // Create a polling timer when server starts (default to 3s)
            PollingPeriod = 3000;
            OnStarted += ArmPollingTimer;
        }

        public void Start(string address, ushort port)
        {
            if (!Active)
            {
                // Create an endpoint to represent this address:port
                _ep = new IPEndPoint(IPAddress.Parse(address), port);

                // Enable the UDP socket
                _sock.JoinMulticastGroup(_ep.Address);
                _sock.Connect(_ep);
                Active = true;

                Console.WriteLine("UDP socket opened on {0}:{1}", _ep.Address, _ep.Port);

                if (OnStarted != null)
                    OnStarted(this, new EventArgs());

                // Setup listening thread
                _sock.BeginReceive(DataReceived, null);
            }
        }

        public void Stop()
        {
            if (Active)
            {
                // Allow clean-up before shutting down socket
                if (OnStopping != null)
                    OnStopping(this, new EventArgs());

                // Disable UDP socket and dispose
                _sock.DropMulticastGroup(_ep.Address);
                _sock.Close();
                _sock.Dispose();
                _sock = null;

                Active = false;

                // Make sure we exit polling loop
                if (_poll != null)
                    _poll.Join();

                Console.WriteLine("UDP socket closed");
            }
        }

        private void DataReceived(IAsyncResult result)
        {
            if (_sock != null)
            {
                // Data available?
                if (_sock.Available > 0)
                {
                    var ep = new IPEndPoint(IPAddress.Any, 0);
                    var data = _sock.EndReceive(result, ref ep);

                    Console.WriteLine("Received {0} bytes from {1}", _sock.Available, ep.Address.ToString());

                    var pkt = JsonConvert.DeserializeObject<DiscoveryPacket>(Encoding.UTF8.GetString(data));

                    Console.WriteLine("  DiscoveryPacket: {0} @ {1} ({2})", pkt.Hostname, pkt.IPv4, pkt.Description);
                }

                // Receive next packet
                if (Active)
                    _sock.BeginReceive(DataReceived, null);
            }
        }

        private void ArmPollingTimer(object sender, EventArgs args)
        {
            _poll = new Thread(PollingLoop);
            _poll.Start();
        }

        private void PollingLoop(object userObj)
        {
            // Create a new DiscoveryPacket with our info
            var pkt = new DiscoveryPacket()
            {
                Version = 1,
                Hostname = this.Hostname,
                IPv4 = this.IPv4,
                Description = this.Description
            };

            while (Active)
            {
                var jsonStr = JsonConvert.SerializeObject(pkt);
                var bytes = Encoding.UTF8.GetBytes(jsonStr);

                if (_sock != null)
                {
                    Console.WriteLine("Sending {0} bytes...", bytes.Length);
                    _sock.Send(bytes, bytes.Length);
                }

                // Don't flood the network
                Thread.Sleep(PollingPeriod);
            }
        }
    }
}
