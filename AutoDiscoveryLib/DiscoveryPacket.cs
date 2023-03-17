using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace AutoDiscoveryLib
{
    class DiscoveryPacket
    {
        public string ID { get; set; }
        public string Address { get; set; }
        public uint Clock { get; set; }

        public DiscoveryPacket()
        {
        }

        public byte[] Serialize()
        {
            // Convert strings to UTF8 byte arrays
            var baID = Encoding.UTF8.GetBytes(ID);
            var baAddress = Encoding.UTF8.GetBytes(Address);

            // Get length of packed structure
            int length = baID.Length + sizeof(uint) +
                         baAddress.Length + sizeof(uint) +
                         sizeof(uint) + 2;

            // Build a new byte array to hold serial data
            var data = new byte[length];
            int i = 0;

            // Header byte
            data[i++] = 0x02;

            // ID
            data.SetValue(baID.Length, i);
            i += sizeof(int);
            baID.CopyTo(data, i);
            i += baID.Length;

            // Address
            data.SetValue(baAddress.Length, i);
            i += sizeof(int);
            baAddress.CopyTo(data, i);
            i += baAddress.Length;

            // Clock
            data.SetValue(Clock, i);
            i += sizeof(uint);

            // Footer byte
            data[i++] = 0x03;

            return data;
        }
    }
}