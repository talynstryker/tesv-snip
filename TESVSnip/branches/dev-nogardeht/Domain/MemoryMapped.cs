using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace TESVSnip.Domain
{
    public static class MemoryMapped
    {
        private static readonly byte[] RecByte = new byte[4];

        public static void Read(MemoryMappedViewAccessor reader, byte[] data, ref int positionInFile, uint size)
        {
            reader.ReadArray(positionInFile, data, 0,(int) size);
            positionInFile += (int)size;
        }

        public static UInt32 ReadUInt32(MemoryMappedViewAccessor reader, ref int positionInFile, ref int offset)
        {
            reader.ReadArray(positionInFile, RecByte, offset, 4);
            positionInFile += 4 + offset;
            return BitConverter.ToUInt32(RecByte, 0);
        }

        public static UInt32 ReadUInt16(MemoryMappedViewAccessor reader, ref int positionInFile, ref int offset)
        {
            reader.ReadArray(positionInFile, RecByte, offset, 2);
            positionInFile += 2 + offset;
            return BitConverter.ToUInt16(RecByte, 0);
        }
    }

}
