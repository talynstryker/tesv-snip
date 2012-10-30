using TESVSnip.DotZLib;

namespace TESVSnip.Domain.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using Model;

    /// <summary>
    /// ZLibStreamWrapper
    /// </summary>
    public class SnipStreamWrapper
    {
        private const int MaxBufferSize = 52428800; // 5 Mb = 5242880 / 30 Mb = 31457280 bytes / 50 Mb =52428800 bytes

        public byte[] OutputBuffer;

        private byte[] _bytes2 = new byte[2];

        private byte[] _bytes4 = new byte[4];

        private readonly long _streamSize;

        public uint MaxOutputBufferPosition { get; private set; } // for calculate an optimized buffer size

        public uint OutputBufferLength { get; private set; }

        public uint OutputBufferPosition { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnipStreamWrapper"/> class. 
        /// Create new instance of SnipStreamWrapper
        /// </summary>
        /// <param name="fs">FileStream</param>
        public SnipStreamWrapper(FileStream fs)
        {
            SnipStream = fs;
            _streamSize = SnipStream.Length;
            SnipStream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Allocate the buffers size
        /// </summary>
        public void AllocateBuffers()
        {
            if (OutputBuffer != null) ReleaseBuffers(); 
            OutputBuffer = new byte[MaxBufferSize];
            ResetBuffer();
        }

        /// <summary>
        /// Reset input/Output buffer with zero
        /// </summary>
        public void ResetBuffer()
        {
            ResetBufferSizeAndPosition();
            MaxOutputBufferPosition = 0;
            Array.Clear(OutputBuffer, 0, MaxBufferSize);
        }

        /// <summary>
        /// Reset size and position of input and output buffer
        /// </summary>
        public void ResetBufferSizeAndPosition()
        {
            OutputBufferLength = 0;
            OutputBufferPosition = 0;
        }

        /// <summary>
        /// Release the buffers
        /// </summary>
        public void ReleaseBuffers()
        {
            OutputBuffer = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Gets the file stream
        /// </summary>
        public FileStream SnipStream { get; private set; }

        /// <summary>
        /// Gets the current position in stream
        /// </summary>
        protected long Position
        {
            get
            {
                if (SnipStream != null)
                    return SnipStream.Position;
                return -1;
            }
        }

        /// <summary>
        /// close the file
        /// </summary>
        public void CloseFile()
        {
            if (SnipStream != null)
            {
                SnipStream.Close();
                SnipStream.Dispose();
            }

            SnipStream = null;
        }

        /// <summary>
        /// Gets the End Of File
        /// </summary>
        /// <returns>True if end of file</returns>
        public bool Eof()
        {
            return SnipStream.Position == SnipStream.Length;
        }

        /// <summary>
        /// Jump to position in stream from offset
        /// </summary>
        /// <param name="offset">Offset (can be negative or positive)</param>
        /// <param name="from">Reference points in stream</param>
        /// <exception cref="Exception">An exception if size or position error</exception>
        public void JumpTo(int offset, SeekOrigin from)
        {
            long newPosition = SnipStream.Position;

            if (from == SeekOrigin.Begin) newPosition = offset;
            if (from == SeekOrigin.Current) newPosition += offset;
            if (from == SeekOrigin.End) newPosition += offset;

            if (newPosition > _streamSize)
                throw new Exception(
                    message: string.Format("WARNING: The final position ({0}) is greater than the size of file ({1}).",
                                           arg0: newPosition.ToString(CultureInfo.InvariantCulture),
                                           arg1: _streamSize.ToString(CultureInfo.InvariantCulture)));

            if (newPosition < 0)
                throw new Exception(
                    message: string.Format("WARNING: The position is negative ({0}).",
                                           arg0: newPosition.ToString(CultureInfo.InvariantCulture)));

            SnipStream.Seek(offset, from);
        }

        /// <summary>
        /// Read bytes in stream from the current position
        /// </summary>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>Contains the specified byte array.</returns>
        public byte[] ReadBytes(int count)
        {
            var b = new byte[count];
            SnipStream.Read(b, 0, count);
            return b;
        }

        /// <summary>
        /// Read UInt16 in stream from the current position
        /// </summary>
        /// <returns>An UInt16 number</returns>
        public UInt16 ReadUInt16()
        {
            SnipStream.Read(_bytes2, 0, 2);
            return BitConverter.ToUInt16(_bytes2, 0);
        }

        /// <summary>
        /// Read UInt32 in stream from the current position
        /// </summary>
        /// <returns>An UInt32 number</returns>
        public UInt32 ReadUInt32()
        {
            SnipStream.Read(_bytes4, 0, 4);
            return BitConverter.ToUInt32(_bytes4, 0);
        }

        /// <summary>
        /// Write a string in file
        /// </summary>
        /// <param name="s">String to write</param>
        public void WriteString(string s)
        {
            var b = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                b[i] = (byte) s[i];
            }

            SnipStream.Write(b, 0, s.Length);
        }

        /// <summary>
        /// Write a string into buffer
        /// </summary>
        /// <param name="s">String to write</param>
        public void WriteStringToBuffer(string s)
        {
            foreach (char t in s)
            {
                OutputBuffer[OutputBufferPosition] = (byte) t;
                OutputBufferPosition++;
                OutputBufferLength++;
            }
        }

        /// <summary>
        /// Write UInt16 into buffer
        /// </summary>
        /// <param name="value">A UInt16 value.</param>
        public void WriteUInt16ToBuffer(UInt16 value)
        {
            _bytes2 = TESVSnip.Framework.TypeConverter.s2h(value);
            Array.Copy(_bytes2, 0, OutputBuffer, OutputBufferPosition, 2);
            OutputBufferPosition += 2;
            OutputBufferLength += 2;
        }

        /// <summary>
        /// Write UInt32 into buffer
        /// </summary>
        /// <param name="value">A UInt32 value.</param>
        public void WriteUInt32ToBuffer(UInt32 value)
        {
            _bytes4 = TESVSnip.Framework.TypeConverter.i2h(value);
            Array.Copy(_bytes4, 0, OutputBuffer, OutputBufferPosition, 4);
            OutputBufferPosition += 4;
            OutputBufferLength += 4;
        }

        /// <summary>
        /// Write Int into buffer
        /// </summary>
        /// <param name="value">A Int value.</param>
        public void WriteIntToBuffer(int value)
        {
            _bytes4 = TESVSnip.Framework.TypeConverter.si2h(value);
            Array.Copy(_bytes4, 0, OutputBuffer, OutputBufferPosition, 4);
            OutputBufferPosition += 4;
            OutputBufferLength += 4;
        }

        /// <summary>
        /// Write Int into buffer
        /// </summary>
        /// <param name="value">A Int value.</param>
        public void WriteIntToBuffer(ushort value)
        {
            _bytes4 = TESVSnip.Framework.TypeConverter.si2h(value);
            Array.Copy(_bytes4, 0, OutputBuffer, OutputBufferPosition, 4);
            OutputBufferPosition += 4;
            OutputBufferLength += 4;
        }
        /// <summary>
        /// Write byte[] into buffer
        /// </summary>
        /// <param name="b">An array of bytes.</param>
        /// <param name="sourceIndex">starting index</param>
        public void WriteByteToBuffer(ref byte[] b, long sourceIndex)
        {
            Array.Copy(b, sourceIndex, OutputBuffer, OutputBufferPosition, b.Length);
            OutputBufferPosition += (uint) b.Length;
            OutputBufferLength += (uint) b.Length;
        }

        /// <summary>
        /// Copy the input buffer to oupput buffer
        /// </summary>
        /// <param name="b">The data Size. </param>
        public void CopyOutputBufferToData(ref byte[] b)
        {
            string msg;
            try
            {
                if (OutputBufferLength == 0)
                {
                    msg = "SnipStreamWrapper.CopyOutputBufferToData: Output buffer is empty.";
                    Clipboard.SetText(msg);
                }
                if (OutputBufferLength > 0)
                {
                    //msg = "SnipStreamWrapper.CopyOutputBufferToData: Output buffer is empty.";
                    //Clipboard.SetText(msg);
                    //throw new TESParserException(msg);
                    b = new byte[OutputBufferLength];
                    Array.Copy(OutputBuffer, 0, b, 0, OutputBufferLength);
                }
            }
            catch (Exception ex)
            {
                msg = "SnipStreamWrapper.CopyOutputBufferToData" + Environment.NewLine +
                      "Message: " + ex.Message +
                      Environment.NewLine +
                      "StackTrace: " + ex.StackTrace;
                Clipboard.SetText(msg);
                throw new TESParserException(msg);
            }
        }

        /// <summary>
        /// Write output buffer to file stream
        /// </summary>
        public void WriteBufferToFile()
        {
            if (OutputBufferLength > 0)
                SnipStream.Write(OutputBuffer, 0, (int) OutputBufferLength);
        }

        /// <summary>
        /// Write UInt32 in file
        /// </summary>
        /// <param name="value">A UInt32 value.</param>
        public void WriteUInt32(UInt32 value)
        {
            _bytes4 = TESVSnip.Framework.TypeConverter.i2h(value);
            SnipStream.Write(_bytes4, 0, _bytes4.Length);
        }

        /// Write bytes in file
        /// </summary>
        /// <param name="value">A byte array.</param>
        public void WriteBytes(byte[] b)
        {
            SnipStream.Write(b, 0, b.Length);
        }

    }
}