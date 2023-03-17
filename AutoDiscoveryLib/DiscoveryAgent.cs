using System;
using System.Collections.Generic;
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

        public EventHandler OnStarted;
        public EventHandler OnStopping;

        public DiscoveryAgent()
        {
            // Create a new UDP server with defaults
            _socket = new UDPServer();

            // Register program status changes so we can clean up
            CrestronEnvironment.ProgramStatusEventHandler += ProgramStatusChange;

            // Add a polling timer to when server starts
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
            // Continue polling while server is active
            while (_active)
            {
                CrestronConsole.PrintLine("* polling *");

                // Adjust this interval if too frequent
                CrestronEnvironment.Sleep(3000);
            }

            // Release resources if currently active
            if (_poll != null)
                _poll.Dispose();

            _poll = null;
        }
    }
}