using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;

//TODO: Remove if MemoryMappedFile OK

namespace TESVSnip.Domain
{
    /// <summary>
    /// MemoryMappedFileHelper
    /// </summary>
    public static class MemoryMappedFileHelper
    {
        private static MemoryMappedFile _memoryMappedFile = null;
        private static byte[] _recByte = null;

        public static byte[] FileBuffer = null;
        public static MemoryMappedViewAccessor FileMap = null;
        public static long FilePointer = 0;

        /// <summary>
        /// Open file and map it in memory
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static bool Open(FileInfo fi)
        {
            try
            {
                Release();
                Init();
                int length = (int) fi.Length;
                _memoryMappedFile = MemoryMappedFile.CreateFromFile(fi.FullName, FileMode.Open, null, length);
                FileMap = _memoryMappedFile.CreateViewAccessor(0, length, MemoryMappedFileAccess.Read);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Release();
                return false;
            }
        }

        /// <summary>
        /// Init declaration
        /// </summary>
        private static void Init()
        {
            try
            {
                FileBuffer = new byte[104857600]; //100 Megabyte (MB) = 104 857 600 Byte
                Array.Clear(FileBuffer, 0, FileBuffer.Length);

                _recByte = new byte[4];
                Array.Clear(_recByte, 0, _recByte.Length);

                FilePointer = 0;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Release declaration
        /// </summary>
        private static void Release()
        {
            try
            {
                if (FileBuffer != null) Array.Clear(FileBuffer, 0, FileBuffer.Length);
                FileBuffer = null;

                if (_recByte != null) Array.Clear(_recByte, 0, _recByte.Length);
                _recByte = null;

                if (FileMap != null) FileMap.Dispose();
                FileMap = null;

                if (_memoryMappedFile != null) _memoryMappedFile.Dispose();
                _memoryMappedFile = null;

                //GC.Collect();
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Allocate a buffer Of byte
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] AllocateBufferOfByte(uint size)
        {
            try
            {
                byte[] buffer = new byte[size];
                Array.Clear(buffer, 0, buffer.Length);
                return buffer;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Desallocate a buffer Of byte
        /// </summary>
        /// <param name="buffer"></param>
        public static void FreeBufferOfByte(ref byte[] buffer)
        {
            try
            {
                if (buffer != null) Array.Clear(buffer, 0, buffer.Length);
                buffer = null;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Populate byte array from a MemoryMappedFile
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="data"></param>
        /// <param name="positionInFile"></param>
        /// <param name="size"></param>
        public static void Read(ref MemoryMappedViewAccessor reader, byte[] data, ref int positionInFile, uint size)
        {
            try
            {
                reader.ReadArray(positionInFile, data, 0, (int) size);
                positionInFile += (int) size;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Read x bytes in memory mapped file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        public static byte[] ReadBytes(uint size)
        {
            try
            {
                byte[] data = new byte[size];
                FileMap.ReadArray(FilePointer, data, 0, (int) size);
                FilePointer += (int) size;
                return data;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        /// <summary>
        /// Populate byte array from a buffer of byte
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public static void Read(ref byte[] buffer, ref byte[] data, ref int offset, uint size)
        {
            try
            {
                System.Buffer.BlockCopy(buffer, offset, data, 0, (int) size);
                offset += (int) size;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Return a byte array from a MemoryMappedFile
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="positionInFile"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] ReadByte(ref MemoryMappedViewAccessor reader, ref int positionInFile, uint size)
        {
            try
            {
                byte[] data = new byte[size];
                reader.ReadArray(positionInFile, data, 0, (int) size);
                positionInFile += (int) size;
                return data;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Return a UInt32 from a MemoryMappedFile
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="positionInFile"></param>
        /// <returns></returns>
        public static UInt32 ReadUInt32(ref MemoryMappedViewAccessor reader, ref int positionInFile)
        {
            try
            {
                reader.ReadArray(positionInFile, _recByte, 0, 4);
                positionInFile += 4;
                return BitConverter.ToUInt32(_recByte, 0);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public static UInt32 ReadUInt32()
        {
            try
            {
                //FileMap.ReadArray(FilePointer, _recByte, 0, 4);

                UInt32 value = FileMap.ReadUInt32(FilePointer);
                FilePointer += 4;
                return value;
                //return BitConverter.ToUInt32(_recByte, 0);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Return a UInt32 from a buffer of byte
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static UInt32 ReadUInt32(byte[] buffer, ref int offset)
        {
            try
            {
                System.Buffer.BlockCopy(buffer, offset, _recByte, 0, 4);
                return BitConverter.ToUInt32(_recByte, 0);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Return a UInt16 from a MemoryMappedFile
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="positionInFile"></param>
        /// <returns></returns>
        public static UInt16 ReadUInt16(ref MemoryMappedViewAccessor reader, ref int positionInFile)
        {
            try
            {
                reader.ReadArray(positionInFile, _recByte, 0, 2);
                positionInFile += 2;
                return BitConverter.ToUInt16(_recByte, 0);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        /// <summary>
        /// Return a UInt16 from a buffer of byte
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static UInt16 ReadUInt16(byte[] buffer, ref int offset)
        {
            try
            {
                System.Buffer.BlockCopy(buffer, offset, _recByte, 0, 2);
                offset += 2;
                return BitConverter.ToUInt16(_recByte, 0);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public static UInt16 ReadUInt16()
        {
            try
            {
                UInt16 value = FileMap.ReadUInt16(FilePointer);
                FilePointer += 2;
                return value;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

    }

}