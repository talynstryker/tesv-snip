using Ionic.Zlib;

namespace TESVSnip.Domain.Services
{
    using System;
    using System.IO;

    using TESVSnip.DotZLib;

    public static class ZLib
    {
        public static string Version { get; private set; }

        public static byte[] Compress(Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var buffer = new byte[input.Length];
            input.Read(buffer, 0, (int) input.Length);

            return Compress(buffer);
        }

        public static byte[] Compress(byte[] input)
        {
            using (var output = new MemoryStream())
            {
                using (var deflater = new Deflater(CompressLevel.Best))
                {
                    deflater.DataAvailable += output.Write;
                    deflater.Add(input, 0, input.Length);
                }

                return output.ToArray();
            }
        }

        public static void Initialize()
        {
            Version = Info.Version;
        }

        public static BinaryReader Decompress(Stream input, int expectedSize = 0)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var buffer = new byte[input.Length];
            input.Read(buffer, 0, (int) input.Length);

            return Decompress(buffer, expectedSize);
        }

        private static BinaryReader Decompress(byte[] buffer, int expectedSize = 0)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            var output = new MemoryStream(expectedSize);
            try
            {
                using (var inflater = new Inflater())
                {
                    inflater.DataAvailable += output.Write;
                    inflater.Add(buffer, 0, buffer.Length);
                }

                output.Position = 0;
                return new BinaryReader(output);
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        private static byte[] _inflaterBuffer = null;

        public static byte[] Decompress2(byte[] buffer, int expectedSize = 0)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            //var output = new MemoryStream(expectedSize);
            if (_inflaterBuffer != null) MemoryMappedFileHelper.FreeBufferOfByte(ref _inflaterBuffer);
            _inflaterBuffer = MemoryMappedFileHelper.AllocateBufferOfByte((uint) expectedSize);

            var output = new MemoryStream(expectedSize);

            try
            {
                //using (var inflater = new Inflater())
                //{
                //    //inflater.DataAvailable += output.Write;
                //    inflater.DataAvailable += DataAvailableCallback;
                //    inflater.Add(buffer, 0, buffer.Length);
                //    //inflater.Finish();
                //}

                using (var inflater = new Inflater())
                {
                    inflater.DataAvailable += output.Write;
                    inflater.Add(buffer, 0, buffer.Length);
                }

                output.Position = 0;
                //byte[] buf = new byte[output.BaseStream.Length];
                output.Read(_inflaterBuffer, 0, _inflaterBuffer.Length);

                //output.Position = 0;
                return _inflaterBuffer; //new BinaryReader(output);
            }
            catch (Exception e)
            {
                //output.Dispose();
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="expectedSize"></param>
        /// <returns></returns>
        public static byte[] DecompressIonic(ref byte[] buffer, int expectedSize = 0)
        {
            try
            {
                using (MemoryStream msCompressed = new MemoryStream(buffer))
                {
                    using (MemoryStream msDecompressed = new MemoryStream())
                    {
                        using (ZlibStream zlibStream = new ZlibStream(msDecompressed, CompressionMode.Decompress, true))
                        {
                            msCompressed.Seek(0, SeekOrigin.Begin);
                            CopyStream(msCompressed, zlibStream);
                            zlibStream.Flush();

                            return msDecompressed.ToArray();
                        }
                    }
                }
            }
            catch
                (Exception e)
            {
                throw;
            }
        }

        private static
            void CopyStream
            (System.IO.Stream src, System.IO.Stream dest)
        {
            byte[] buffer = new byte[1024];
            int len = src.Read(buffer, 0, buffer.Length);
            while (len > 0)
            {
                dest.Write(buffer, 0, len);
                len = src.Read(buffer, 0, buffer.Length);
            }
            dest.Flush();
        }

    }
}