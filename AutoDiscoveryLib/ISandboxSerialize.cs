using System;

namespace AutoDiscoveryLib
{
    public enum SandboxSerializeDataType : byte
    {
        Start = 1,
        Int32,
        UInt32,
        String,
        End = 127
    }

    public interface ISandboxSerialize
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
        void Deserialize(byte[] data, int numBytes);
    }
}