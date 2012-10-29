using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using TESVSnip.Domain.Model;

namespace TESVSnip.Domain.Services
{
    public enum BufferType
    {
        Input = 0,
        Output
    }

    /// <summary>
    /// ZLibStreamWrapper
    /// </summary>
    public static class ZLibStreamWrapper
    {
        private const int MaxBufferSize = 5242880; //50 Mb =52428800 bytes     30 Mb = 31457280 bytes  5 Mb = 5242880

        public static byte[] InputBuffer;
        public static byte[] OutputBuffer;

        private static readonly byte[] Bytes2 = new byte[2];
        private static readonly byte[] Bytes4 = new byte[4];

        public static List<string> CompressedRecords { get; private set; } //list of compressed records
        public static List<string> AllRecords { get; private set; } //list of all records

        public static uint InputBufferLength { get; private set; }

        public static uint InputBufferPosition { get; private set; }

        public static uint MaxOutputBufferPosition { get; private set; } //for calculate an optimized buffer size

        public static uint OutputBufferLength { get; private set; }

        public static uint OutputBufferPosition { get; private set; }

        /// <summary>
        /// Add a new record in list of compressed records
        /// </summary>
        public static void AddRecordToCompressedRecordsList(string recordName)
        {
            if (CompressedRecords.IndexOf(recordName, 0) == -1)
            {
                CompressedRecords.Add(recordName);
                CompressedRecords.Add(Environment.NewLine);
            }
        }

        /// <summary>
        /// Add a new record in list of compressed records
        /// </summary>
        public static void AddRecordToRecordsList(string recordName)
        {
            if (AllRecords.IndexOf(recordName, 0) == -1)
            {
                AllRecords.Add(recordName);
                AllRecords.Add(Environment.NewLine);
            }
        }

        /// <summary>
        /// Allocate the buffers size
        /// </summary>
        public static void AllocateBuffers()
        {
            if (InputBuffer != null | OutputBuffer != null) ReleaseBuffers();
            InputBuffer = new byte[MaxBufferSize];
            OutputBuffer = new byte[MaxBufferSize];
            if (CompressedRecords == null) CompressedRecords = new List<string>();
            CompressedRecords.Clear();
            if (AllRecords == null) AllRecords = new List<string>();
            AllRecords.Clear();
            MaxOutputBufferPosition = 0;
            ResetBuffer();
        }

        /// <summary>
        /// Empty input/Output buffer
        /// </summary>
        public static void Clear()
        {
            InputBufferLength = 0;
            OutputBufferLength = 0;
            InputBufferPosition = 0;
            OutputBufferPosition = 0;
        }

        /// <summary>
        /// Copy the input buffer to oupput buffer
        /// </summary>
        public static void CopyInputBufferToOutputBuffer(uint dataSize)
        {
            string msg;
            try
            {
                if (InputBufferLength == 0)
                {
                    msg = "ZLibStreamWrapper.CopyBufferInputToBufferOupput: Static input buffer is empty.";
                    Clipboard.SetText(msg);
                    throw new TESParserException(msg);
                }

                if (dataSize > MaxBufferSize)
                {
                    msg = string.Format(
                        "ZLibStreamWrapper.CopyBufferInputToBufferOupput: Static buffer is too small. DataSize={0}  /  Buffer={1}",
                        dataSize.ToString(CultureInfo.InvariantCulture)
                        , MaxBufferSize.ToString(CultureInfo.InvariantCulture));
                    Clipboard.SetText(msg);
                    throw new TESParserException(msg);
                }

                if (InputBufferLength > 0)
                    Array.Copy(InputBuffer, OutputBuffer, dataSize);


            }
            catch (Exception ex)
            {
                msg = "ZLibStreamWrapper.CopyInputBufferToOutputBuffer" + Environment.NewLine +
                      "Message: " + ex.Message +
                      Environment.NewLine +
                      "StackTrace: " + ex.StackTrace;
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }
            finally
            {
                OutputBufferLength = InputBufferLength;
                OutputBufferPosition = 0;
                InputBufferPosition = 0;
            }
        }

        /// <summary>
        /// Copy portion of stream in buffre
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="bytesToRead"></param>
        public static void CopyTo(FileStream fs, uint bytesToRead)
        {
            InputBufferLength = 0;
            OutputBufferLength = 0;
            OutputBufferPosition = 0;

            if ((fs.Position + bytesToRead) > fs.Length)
            {
                string msg = "ZLibStreamWrapper.CopyTo: ZLib inflate error. Copy size " +
                             (fs.Position + bytesToRead).ToString(CultureInfo.InvariantCulture) +
                             " is over stream length " +
                             fs.Length.ToString(CultureInfo.InvariantCulture);
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            if (bytesToRead > MaxBufferSize)
            {
                string msg = "ZLibStreamWrapper.CopyTo: ZLib inflate error. Bytes to read (" +
                             bytesToRead.ToString(CultureInfo.InvariantCulture) + ")" +
                             " exceed buffer size ( " + MaxBufferSize.ToString(CultureInfo.InvariantCulture) + ")";
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            Clear();
            var numBytesAddressing = (int)bytesToRead;
            var offset = 0;
            while (numBytesAddressing > 0u)
            {
                var numBytes = (uint)Math.Min(numBytesAddressing, 8192u); //8192u 65536u
                int bytesRead = fs.Read(InputBuffer, offset, (int)numBytes);
                offset += bytesRead;
                numBytesAddressing -= bytesRead;
            }

            InputBufferLength = bytesToRead;
        }

        /// <summary>
        /// Set position in input buffer
        /// </summary>
        /// <param name="position">New position in buffer</param>
        /// <param name="bufferType">Buffer type (input or output)</param>
        public static void Position(uint position, BufferType bufferType)
        {
            if (position < 0u)
            {
                const string msg = "ZLibStreamWrapper.Position: The position cannot be negative.";
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            uint bufferSize = bufferType == BufferType.Output ? OutputBufferLength : InputBufferLength;

            if (position > bufferSize)
            {
                string msg =
                    string.Format(
                        "ZLibStreamWrapper.SetPositionInInputBuffer: The position cannot be greater than buffer size ({0}).",
                        bufferSize.ToString(CultureInfo.InvariantCulture));
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            if (bufferType == BufferType.Output)
                OutputBufferPosition = position;
            else
                InputBufferPosition = position;
        }

        /// <summary>
        /// Read 2 bytes in output buffer from the current position
        /// </summary>
        /// <returns>Contains the specified byte array.</returns>
        public static byte[] Read2Bytes()
        {
            if ((OutputBufferPosition + 2) > OutputBufferLength)
            {
                string msg = string.Format(
                    "ZLibStreamWrapper.Read2Bytes: ZLib inflate error. The final position ({0}) in output buffer is over the buffer size {1}",
                    (OutputBufferPosition + 2).ToString(CultureInfo.InvariantCulture),
                    OutputBufferLength.ToString(CultureInfo.InvariantCulture));
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            Array.Copy(OutputBuffer, OutputBufferPosition, Bytes2, 0, 2);
            OutputBufferPosition += 2;
            return Bytes2;
        }

        /// <summary>
        /// Read 4 bytes in output buffer from the current position
        /// </summary>
        /// <returns>Contains the specified byte array.</returns>
        public static byte[] Read4Bytes()
        {
            if ((OutputBufferPosition + 4) > OutputBufferLength)
            {
                string msg = string.Format(
                    "ZLibStreamWrapper.Read4Bytes: ZLib inflate error. The final position ({0}) in output buffer is over the buffer size {1}",
                    (OutputBufferPosition + 4).ToString(CultureInfo.InvariantCulture),
                    OutputBufferLength.ToString(CultureInfo.InvariantCulture));
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            Array.Copy(OutputBuffer, OutputBufferPosition, Bytes4, 0, 4);
            OutputBufferPosition += 4;
            return Bytes4;
        }

        /// <summary>
        /// Read bytes in input/output buffer from the current position
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to read.
        /// </param>
        /// <param name="bufferType">
        /// Read in input or output buffer
        /// </param>
        /// <returns>
        /// Contains the specified byte array.
        /// </returns>
        public static void ReadBytes(ref byte[] data, int count, BufferType bufferType)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            uint newPosition;
            uint bufferSize;

            if (bufferType == BufferType.Output)
            {
                newPosition = (uint)(OutputBufferPosition + count);
                bufferSize = OutputBufferLength;
            }
            else
            {
                newPosition = (uint)(InputBufferPosition + count);
                bufferSize = InputBufferLength;
            }

            if (newPosition > bufferSize)
            {
                string msg = string.Format(
                    "ZLibStreamWrapper.ReadBytes: ZLib inflate error. The final position ({0}) in {1} buffer is over the buffer size {2}",
                    newPosition.ToString(CultureInfo.InvariantCulture),
                    bufferType.ToString(),
                    bufferSize.ToString(CultureInfo.InvariantCulture));
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            //var b = new byte[count];

            if (bufferType == BufferType.Output)
            {
                //Array.Copy(OutputBuffer, OutputBufferPosition, b, 0, count);
                Array.Copy(OutputBuffer, OutputBufferPosition, data, 0, count);                
                OutputBufferPosition += (uint)count;
            }
            else
            {
                //Array.Copy(InputBuffer, InputBufferPosition, b, 0, count);
                Array.Copy(InputBuffer, InputBufferPosition, data, 0, count);
                InputBufferPosition += (uint)count;
            }

            //return b;
        }

        /// <summary>
        /// Read bytes in input/output buffer from the current position
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to read.
        /// </param>
        /// <param name="bufferType">
        /// Read in input or output buffer
        /// </param>
        /// <returns>
        /// Contains the specified byte array.
        /// </returns>
        public static byte[] ReadBytes(int count, BufferType bufferType)
        {
            uint newPosition;
            uint bufferSize;

            if (bufferType == BufferType.Output)
            {
                newPosition = (uint)(OutputBufferPosition + count);
                bufferSize = OutputBufferLength;
            }
            else
            {
                newPosition = (uint)(InputBufferPosition + count);
                bufferSize = InputBufferLength;
            }

            if (newPosition > bufferSize)
            {
                string msg = string.Format(
                    "ZLibStreamWrapper.ReadBytes: ZLib inflate error. The final position ({0}) in {1} buffer is over the buffer size {2}",
                    newPosition.ToString(CultureInfo.InvariantCulture),
                    bufferType.ToString(),
                    bufferSize.ToString(CultureInfo.InvariantCulture));
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            var b = new byte[count];

            if (bufferType == BufferType.Output)
            {
                Array.Copy(OutputBuffer, OutputBufferPosition, b, 0, count);
                OutputBufferPosition += (uint)count;
            }
            else
            {
                Array.Copy(InputBuffer, InputBufferPosition, b, 0, count);
                InputBufferPosition += (uint)count;
            }

            return b;
        }

        /// <summary>
        /// Read UInt16 in output buffer from the current position
        /// </summary>
        /// <returns>An UInt16 number</returns>
        public static UInt16 ReadUInt16()
        {
            return BitConverter.ToUInt16(Read2Bytes(), 0);
        }

        /// <summary>
        /// Read UInt32 in output buffer from the current position
        /// </summary>
        /// <returns>An UInt32 number</returns>
        public static UInt32 ReadUInt32()
        {
            return BitConverter.ToUInt32(Read4Bytes(), 0);
        }

        /// <summary>
        /// Release the buffers
        /// </summary>
        public static void ReleaseBuffers()
        {
            InputBuffer = null;
            OutputBuffer = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Reset input/Output buffer with zero
        /// </summary>
        public static void ResetBuffer()
        {
            InputBufferLength = 0;
            OutputBufferLength = 0;
            InputBufferPosition = 0;
            OutputBufferPosition = 0;
            MaxOutputBufferPosition = 0;

            Array.Clear(InputBuffer, 0, MaxBufferSize);
            Array.Clear(OutputBuffer, 0, MaxBufferSize);
        }
        /// <summary>
        /// Write in the output buffer the ZLib inflate bytes byte[] data, int startIndex, int count
        /// </summary>
        /// <param name="startIndex"> </param>
        /// <param name="count"></param>
        /// <param name="data"> </param>
        public static void WriteInOutputBuffer(byte[] data, int startIndex, int count)
        {
            if (startIndex < 0)
            {
                const string msg = "ZLibStreamWrapper.WriteInOutputBuffer: The position cannot be negative.";
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            if (count < 0)
            {
                const string msg = "ZLibStreamWrapper.WriteInOutputBuffer: The number of bytes to write cannot be negative.";
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            if ((startIndex + count) > data.Length)
            {
                string msg =
                    string.Format(
                        "ZLibStreamWrapper.WriteInOutputBuffer: The starting position is {0}. A reading of {1} bytes is requested. The number of bytes that can be read in the buffer is {2}",
                        startIndex.ToString(CultureInfo.InvariantCulture),
                        count.ToString(CultureInfo.InvariantCulture),
                        (data.Length - startIndex).ToString(CultureInfo.InvariantCulture));

                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            if ((OutputBufferPosition + count) > MaxBufferSize)
            {
                string msg =
                    string.Format(
                        "ZLibStreamWrapper.WriteInOutputBuffer: The output buffer is to small. Buffer size={0}, Number of bytes to write={1}",
                        MaxBufferSize.ToString(CultureInfo.InvariantCulture),
                        (OutputBufferPosition + count).ToString(CultureInfo.InvariantCulture));
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }

            Array.Copy(data, startIndex, OutputBuffer, OutputBufferPosition, count);
            OutputBufferPosition += (uint)count;
            OutputBufferLength = OutputBufferPosition;
            if (OutputBufferPosition > MaxOutputBufferPosition) MaxOutputBufferPosition = OutputBufferPosition;
        }
    }
}