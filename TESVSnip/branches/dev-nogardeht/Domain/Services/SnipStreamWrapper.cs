namespace TESVSnip.Domain.Services
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// ZLibStreamWrapper
    /// </summary>
    public class SnipStreamWrapper
    {
        private readonly byte[] _bytes2 = new byte[2];

        private readonly byte[] _bytes4 = new byte[4];

        private readonly long _streamSize;

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
        /// The file stream
        /// </summary>
        public FileStream SnipStream { get; set; }

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
    }
}