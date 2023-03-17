using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace AutoDiscoveryLib
{
    public class DiscoveryAgent
    {
        private UDPServer _socket;

        private bool _isActive;
        public ushort Active
        {
            get
            {
                if (_isActive)
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
        }

        public void Start(string address, ushort port)
        {
            var result = _socket.EnableUDPServer(address, port);

            if (result == SocketErrorCodes.SOCKET_OK)
                _isActive = true;
            else
                _isActive = false;

            CrestronConsole.PrintLine("Start result = {0}", result.ToString());

            // Allow first-run actions once socket is established
            if (OnStarted != null && _isActive)
                OnStarted(this, new EventArgs());
        }

        public void Stop()
        {
            if (_isActive)
            {
                // Allow clean-up before shutting down socket
                if (OnStopping != null)
                    OnStopping(this, new EventArgs());

                var result = _socket.DisableUDPServer();

                CrestronConsole.PrintLine("Stop result = {0}", result.ToString());
            }

            _isActive = false;
        }
    }
}