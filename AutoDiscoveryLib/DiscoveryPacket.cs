using System;
using System.Text;
using System.Runtime;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace AutoDiscoveryLib
{
    class DiscoveryPacket : ISandboxSerialize
    {
        public string ID { get; set; }
        public string Address { get; set; }
        public uint Clock { get; set; }

        public byte[] Serialize()
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            // Start
            writer.Write((int)SandboxSerializeDataType.StartMarker);

            // ID
            writer.Write((int)SandboxSerializeDataType.String);
            var baID = Encoding.UTF8.GetBytes(ID);
            writer.Write(baID.Length);
            writer.Write(baID);

            // Address
            writer.Write((int)SandboxSerializeDataType.String);
            var baAddress = Encoding.UTF8.GetBytes(Address);
            writer.Write(baAddress.Length);
            writer.Write(baAddress);

            // Clock
            writer.Write((int)SandboxSerializeDataType.UInt32);
            writer.Write(Clock);

            // End
            writer.Write((int)SandboxSerializeDataType.EndMarker);
            writer.Flush();

            return ms.GetBuffer();
        }

        public void Deserialize(byte[] data)
        {
            Deserialize(data, data.Length);
        }

        public void Deserialize(byte[] data, int numBytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(data, 0, numBytes)))
            {
                int length;

                // Start
                if (reader.ReadInt32() != (int)SandboxSerializeDataType.StartMarker)
                    throw new Exception("StartMarker missing");

                // ID
                if (reader.ReadInt32() != (int)SandboxSerializeDataType.String)
                    throw new Exception("Expected String marker for ID");
                length = reader.ReadInt32();
                ID = Encoding.UTF8.GetString(reader.ReadBytes(length), 0, length);

                // Address
                if (reader.ReadInt32() != (int)SandboxSerializeDataType.String)
                    throw new Exception("Expected String marker for Address");
                length = reader.ReadInt32();
                Address = Encoding.UTF8.GetString(reader.ReadBytes(length), 0, length);

                // Clock
                if (reader.ReadInt32() != (int)SandboxSerializeDataType.UInt32)
                    throw new Exception("Expected UInt32 marker for Clock");
                Clock = reader.ReadUInt32();

                // End
                if (reader.ReadInt32() != (int)SandboxSerializeDataType.EndMarker)
                    throw new Exception("EndMarker missing");
            }
        }
    }
}