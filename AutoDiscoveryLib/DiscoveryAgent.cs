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
        }

        public void Stop()
        {
            if (_isActive)
            {
                var result = _socket.DisableUDPServer();

                CrestronConsole.PrintLine("Stop result = {0}", result.ToString());
            }

            _isActive = false;
        }
    }
}