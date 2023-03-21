using System;
using System.Text;
using System.Runtime;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace AutoDiscoveryLib
{
    class DiscoveryPacket : ISandboxSerialize
    {
        bool _dirty;
        byte[] _bytes;
        int _numBytes;

        int _version;
        public int Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                _dirty = true;
            }
        }

        string _ipv4;
        public string IPv4
        {
            get
            {
                return _ipv4;
            }
            set
            {
                _ipv4 = value;
                _dirty = true;
            }
        }

        string _hostname;
        public string Hostname
        {
            get
            {
                return _hostname;
            }
            set
            {
                _hostname = value;
                _dirty = true;
            }
        }

        string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                _dirty = true;
            }
        }

        public DiscoveryPacket()
        {
            _dirty = true;
        }

        public byte[] Serialize()
        {
            if (_dirty)
            {
                using (var ms = new MemoryStream())
                {
                    _dirty = false;

                    using (var writer = new BinaryWriter(ms))
                    {
                        // Start
                        writer.Write((byte)SandboxSerializeDataType.Start);

                        // Version
                        writer.Write((byte)SandboxSerializeDataType.Int32);
                        writer.Write(Version);

                        // IPv4
                        if (IPv4 == null)
                            IPv4 = String.Empty;
                        writer.Write((byte)SandboxSerializeDataType.String);
                        WriteBytes(writer, Encoding.UTF8.GetBytes(IPv4));

                        // Hostname
                        if (Hostname == null)
                            Hostname = String.Empty;
                        writer.Write((byte)SandboxSerializeDataType.String);
                        WriteBytes(writer, Encoding.UTF8.GetBytes(Hostname));

                        // Description
                        if (Description == null)
                            Description = String.Empty;
                        writer.Write((byte)SandboxSerializeDataType.String);
                        WriteBytes(writer, Encoding.UTF8.GetBytes(Description));

                        // End
                        writer.Write((byte)SandboxSerializeDataType.End);

                        _numBytes = (int)writer.BaseStream.Position;
                        _bytes = ms.ToArray();

                        CrestronConsole.PrintLine("Sending packet size should be {0} bytes...", _numBytes);
                    }
                }
            }

            return _bytes;
        }

        private void WriteBytes(BinaryWriter writer, byte[] bytes)
        {
            writer.Write(bytes.Length);
            writer.Write(bytes);
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
                if (reader.ReadByte() != (byte)SandboxSerializeDataType.Start)
                    throw new Exception("Start byte missing");

                // Version
                if (reader.ReadByte() != (byte)SandboxSerializeDataType.Int32)
                    throw new Exception("Expected Int32 marker for Version");
                Version = reader.ReadInt32();

                // IPv4
                if (reader.ReadByte() != (byte)SandboxSerializeDataType.String)
                    throw new Exception("Expected String marker for IPv4");
                length = reader.ReadInt32();
                IPv4 = Encoding.UTF8.GetString(reader.ReadBytes(length), 0, length);

                // Hostname
                if (reader.ReadByte() != (byte)SandboxSerializeDataType.String)
                    throw new Exception("Expected String marker for Hostname");
                length = reader.ReadInt32();
                Hostname = Encoding.UTF8.GetString(reader.ReadBytes(length), 0, length);

                // Description
                if (reader.ReadByte() != (byte)SandboxSerializeDataType.String)
                    throw new Exception("Expected String marker for Description");
                length = reader.ReadInt32();
                Description = Encoding.UTF8.GetString(reader.ReadBytes(length), 0, length);

                // End
                if (reader.ReadByte() != (byte)SandboxSerializeDataType.End)
                    throw new Exception("End byte missing");
            }
        }
    }
}