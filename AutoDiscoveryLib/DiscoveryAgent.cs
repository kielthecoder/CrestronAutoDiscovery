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
            _socket = new UDPServer();

            OnStarted += ArmPollingTimer;
        }

        public void Start(string address, ushort port)
        {
            // Make sure server is stopped first
            Stop();

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

                var result = _socket.DisableUDPServer();

                CrestronConsole.PrintLine("Stop result = {0}", result.ToString());
            }

            _active = false;
        }

        private void ArmPollingTimer(object sender, EventArgs args)
        {
            if (_poll != null)
                _poll.Dispose();

            _poll = new CTimer(PollingLoop, 0);
        }

        private void PollingLoop(object userObj)
        {
            while (_active)
            {
                CrestronConsole.PrintLine("* polling *");

                CrestronEnvironment.Sleep(3000);
            }

            if (_poll != null)
                _poll.Dispose();

            _poll = null;
        }
    }
}