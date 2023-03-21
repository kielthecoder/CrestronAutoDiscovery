using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace AutoDiscoveryLib
{
    public class DiscoveryAgent
    {
        private UDPServer _socket;
        private CTimer _poll;

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

        public EventHandler OnStarted;
        public EventHandler OnStopping;

        public DiscoveryAgent()
        {
            // Create a new UDP server with defaults
            _socket = new UDPServer();

            // Register program status changes so we can clean up
            CrestronEnvironment.ProgramStatusEventHandler += ProgramStatusChange;

            // Add a polling timer to when server starts
            PollingPeriod = 3000;
            OnStarted += ArmPollingTimer;
        }

        public void Start(string address, ushort port)
        {
            // Make sure server is stopped first
            Stop();

            // Enable the UDP socket for given address and port
            var result = _socket.EnableUDPServer(address, port);

            if (result == SocketErrorCodes.SOCKET_OK)
                _active = true;
            else
                _active = false;

            CrestronConsole.PrintLine("Start result = {0}", result.ToString());

            // Allow first-run actions once socket is established
            if (OnStarted != null && _active)
                OnStarted(this, new EventArgs());

            // Setup listening thread
            _socket.ReceiveDataAsync(DataReceived);
        }

        public void Stop()
        {
            if (_active)
            {
                // Allow clean-up before shutting down socket
                if (OnStopping != null)
                    OnStopping(this, new EventArgs());

                // Disable UDP socket and release resources
                var result = _socket.DisableUDPServer();

                CrestronConsole.PrintLine("Stop result = {0}", result.ToString());
            }

            _active = false;
        }

        private void DataReceived(UDPServer server, int numBytes)
        {
            if (numBytes > 0)
            {
                try
                {
                    CrestronConsole.PrintLine("Received {0} bytes...", numBytes);

                    var pkt = new DiscoveryPacket();
                    pkt.Deserialize(server.IncomingDataBuffer, numBytes);

                    CrestronConsole.PrintLine("Discovery packet received: {0} @ {1} ({2})", pkt.Hostname, pkt.IPv4, pkt.Description);
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("Exception in DataReceived: {0}", e.Message);
                }
            }

            // Re-arm listening thread
            if (_active)
                _socket.ReceiveDataAsync(DataReceived);
        }

        private void ProgramStatusChange(eProgramStatusEventType type)
        {
            if (type == eProgramStatusEventType.Stopping)
            {
                // Stop server if program is shutting down
                Stop();

                // Wait for polling loop to exit gracefully
                while (_poll != null)
                    CrestronEnvironment.Sleep(100);

                // OK program can stop now
            }
        }

        private void ArmPollingTimer(object sender, EventArgs args)
        {
            // Make sure we release resources if currently active
            if (_poll != null)
                _poll.Dispose();

            // Start a new thread for polling
            _poll = new CTimer(PollingLoop, 0);
        }

        private void PollingLoop(object userObj)
        {
            // Create a new DiscoveryPacket with our info
            var pkt = new DiscoveryPacket() { Version = 1, Hostname = this.Hostname, IPv4 = this.IPv4, Description = this.Description };
            
            // Continue polling while server is active
            while (_active)
            {
                try
                {
                    // Serialize our object for transmisison
                    var bytes = pkt.Serialize();

                    // Send our serialized info to the network
                    if (bytes != null)
                        _socket.SendData(bytes, bytes.Length);
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("Exception in poll loop: {0}", e.Message);

                    if (e.InnerException != null)
                        CrestronConsole.PrintLine("\t{0}", e.InnerException.Message);
                }

                // Adjust this interval if too frequent
                CrestronEnvironment.Sleep(PollingPeriod);
            }

            // Release resources if currently active
            if (_poll != null)
                _poll.Dispose();

            _poll = null;
        }
    }
}