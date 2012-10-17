using System.Windows.Forms;

namespace TESVSnip.Domain.Services
{
    using System;
    using System.IO;

    using TESVSnip.DotZLib;

    public static class ZLib
    {
        public static void Initialize()
        {
            Version = Info.Version;
        }

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

        public static BinaryReader Decompress(Stream input, int expectedSize = 0)
        {
            Byte[] buffer = null;
            try
            {

                if (input == null)
                {
                    throw new ArgumentNullException("input");
                }

                buffer = new byte[input.Length];
                input.Read(buffer, 0, (int) input.Length);

                return Decompress(buffer, expectedSize);

            }
            catch (Exception ex)
            {
                if (buffer != null)
                    File.WriteAllBytes("c:\\DecompressStream_input.txt", buffer);
                return null;
                throw;
            }
        }

        private static BinaryReader Decompress(byte[] buffer, int expectedSize = 0)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            MemoryStream output = null;

            try
            {
                output = new MemoryStream(expectedSize);
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }

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
                File.WriteAllBytes("c:\\buffer.txt", buffer);
                output.Dispose();
                throw;
            }
        }

        public static BinaryReader DecompressSmaugVersion1(Stream input, uint numBytesAddressing, uint expectedSize = 0)
        {
            Byte[] buffer = null;


            // *** Retrieve the compression level
            //long initialPos = input.Position;
            //BinaryReader br = new BinaryReader(input);
            //ushort flags = BitConverter.ToUInt16(br.ReadBytes(2), 0);
            //br.BaseStream.Position = initialPos;
            //var b1 = br.ReadBytes(1);
            //CompressLevel compressionLevel = RetrieveCompressionLevel(flags);
            //br.BaseStream.Position = initialPos;

            
            Inflater inflater = new Inflater();
            ZLibMemoryStreamWrapper zlibMemoryStreamWrapper = new ZLibMemoryStreamWrapper();
            inflater.DataAvailable += delegate(byte[] data, int startPos, int count)
            {
                zlibMemoryStreamWrapper.Write(data, startPos, count);
            };

            BinaryReader br = new BinaryReader(input);
            while (numBytesAddressing > 0u)
            {
                uint numBuffer =Math.Min(numBytesAddressing, 16384u); //calc buffer size    //  65536u); //16384
                inflater.Add(br.ReadBytes((int)numBuffer));
                numBytesAddressing -= numBuffer;
            }
            br.Close();
            br.Dispose();

            inflater.Finish(); //flush zlib buffer



            inflater.Dispose();
            inflater = null;

            zlibMemoryStreamWrapper.Close();

            zlibMemoryStreamWrapper.Position = 0u;

            return new BinaryReader(zlibMemoryStreamWrapper);

            //return br;
            //try
            //{

            //    if (input == null)
            //    {
            //        throw new ArgumentNullException("input");
            //    }

            //    buffer = new byte[input.Length];
            //    input.Read(buffer, 0, (int)input.Length);

            //    return Decompress(buffer, expectedSize);

            //}
            //catch (Exception ex)
            //{
            //    if (buffer != null)
            //        File.WriteAllBytes("c:\\DecompressStream_input.txt", buffer);
            //    return null;
            //    throw;
            //}
        }

        public static BinaryReader DecompressSmaugVersion2(Stream input, int expectedSize = 0)
        {
            Byte[] buffer = null;
            uint numBytesAddressing = (uint) input.Length;
            MemoryStream output = new MemoryStream(expectedSize);
            Inflater inflater = new Inflater();

            try
            {
                inflater.DataAvailable += output.Write;

                BinaryReader br = new BinaryReader(input);
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                while (numBytesAddressing > 0u)
                {
                    uint numBytes = Math.Min(numBytesAddressing, 8192u); //calc buffer size    //  65536u); //16384
                    inflater.Add(br.ReadBytes((int) numBytes));
                    numBytesAddressing -= numBytes;
                }

                inflater.Finish(); //flush zlib buffer

                br.Close();
                br.Dispose();

                output.Seek(0, SeekOrigin.Begin);

                inflater.Dispose();

                return new BinaryReader(output);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error DecompressSmaugVersion2: \n" + ex, "Decompress", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Retrieve the compression level of the stream
        /// RFC 1950
        /// 0       1  
        /// +---+---+
        /// |CMF|FLG|   (more-->)
        /// +---+---+
        /// 
        /// FLG (FLaGs) 
        /// This flag byte is divided as follows: 
        /// 
        /// bits 0 to 4  FCHECK  (check bits for CMF and FLG)
        /// bit  5       FDICT   (preset dictionary)
        /// bits 6 to 7  FLEVEL  (compression level)
        /// 
        /// 	public enum CompressLevel
        /// 	{
        /// 	Default = -1,
        /// 	None,
        /// 	Best = 9,
        /// 	Fastest = 1
        /// 	}
        /// </summary>
        /// <param name="byteSwappedHeader"></param>
        /// <returns></returns>

        private static CompressLevel RetrieveCompressionLevel(ushort flags)
        {
            //ushort num = (ushort)((65280 & flags) >> 8 | (int)(255 & flags) << 8);
            var bit6 = (flags & (1 << 6 - 1)) != 0;
            var bit7 = (flags & (1 << 7 - 1)) != 0;

            //switch ((ushort)(num >> 6 & 3))
            //{
            //    case 2:
            //        return CompressLevel.Default;
            //    case 3:
            return CompressLevel.Best;
            //    default:
            //        throw new Exception("Unknown Zlib header format or unexpected compression level");
            //}
        }

    }
}
