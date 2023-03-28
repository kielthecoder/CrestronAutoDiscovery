using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Crestron.SimplSharp;

namespace AutoDiscoveryLib4
{
    public class DiscoveryAgent
    {
        private System.Net.IPEndPoint _endpoint;
        private UdpClient _socket;
        private Thread _poll;

        private bool _active;
        public ushort Active
        {
            get
            {
                // Translate bool to ushort for SIMPL+
                if (_active)
                    return 1;
                else
                    return 0;
            }
        }

        public string IPv4 { get; set; }
        public string Hostname { get; set; }
        public string Description { get; set; }
        public int PollingPeriod { get; set; }

        public event EventHandler OnStarted;
        public event EventHandler OnStopping;

        public DiscoveryAgent()
        {
            // Create a new UDP socket with defaults
            _socket = new UdpClient();

            // Add a polling timer to when server starts
            PollingPeriod = 3000;
            OnStarted += ArmPollingTimer;
        }

        public void Start(string address, ushort port)
        {
            // Make sure server is stopped first
            Stop();

            // Create an endpoint to represent this address:port
            _endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(address), port);

            // Enable the UDP socket for given address and port
            _socket.Connect(_endpoint);

            // Always succeeds?
            _active = true;

            //Console.WriteLine("UDP socket opened");
            CrestronConsole.PrintLine("UDP socket opened");

            if (OnStarted != null && _active)
                OnStarted(this, new EventArgs());

            // Setup listening thread
            _socket.BeginReceive(DataReceived, _socket);
        }

        public void Stop()
        {
            if (_active)
            {
                // Allow clean-up before shutting down socket
                if (OnStopping != null)
                    OnStopping(this, new EventArgs());

                // Disable the UDP socket and release resources
                _socket.Close();
                _socket.Dispose();

                _active = false;

                // Make sure we leave the polling loop
                if (_poll != null)
                    _poll.Join();

                CrestronConsole.PrintLine("UDP socket stopped");
            }
        }

        private void DataReceived(IAsyncResult result)
        {
            var sock = (UdpClient)result.AsyncState;
            if (sock != null)
            {
                if (sock.Available > 0)
                {
                    var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                    var data = sock.EndReceive(result, ref ep);

                    //Console.WriteLine("Received {0} bytes from {1}...", sock.Available, ep.Address.ToString());
                    CrestronConsole.PrintLine("Received {0} bytes from {1}...", sock.Available, ep.Address.ToString());

                    var fmt = new BinaryFormatter();
                    var pkt = (DiscoveryPacket)fmt.Deserialize(new MemoryStream(data));

                    //Console.WriteLine("Discovery packet received: {0} @ {1} ({2})", pkt.Hostname, pkt.IPv4, pkt.Description);
                    CrestronConsole.PrintLine("  DiscoveryPacket: {0} @ {1} ({2})", pkt.Hostname, pkt.IPv4, pkt.Description);
                }
            }

            // Re-arm listening thread
            if (_active)
                _socket.BeginReceive(DataReceived, _socket);
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
                { Version = 1, Hostname = this.Hostname, IPv4 = this.IPv4, Description = this.Description };

            var fmt = new BinaryFormatter();

            // Continue polling while server is active
            while (_active)
            {
                var stream = new MemoryStream();
                
                // Serialize our DiscoveryPacket into a stream of bytes
                fmt.Serialize(stream, pkt);
                _socket.Send(stream.GetBuffer(), (int)stream.Length);

                // Adjust the interval if too frequent
                Thread.Sleep(PollingPeriod);
            }
        }
    }
}
