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
            uint numBytesAddressing = (uint) input.Length;
            MemoryStream output = null;
            Inflater inflater = null;
            BinaryReader br = null;

            try
            {
                inflater = new Inflater();
                output = new MemoryStream(expectedSize);
                br = new BinaryReader(input);

                inflater.DataAvailable += output.Write;
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                while (numBytesAddressing > 0u)
                {
                    uint numBytes = Math.Min(numBytesAddressing, 8192u); //calc buffer size    //  65536u); //16384
                    inflater.Add(br.ReadBytes((int) numBytes));
                    numBytesAddressing -= numBytes;
                }

                inflater.Finish(); //flush zlib buffer

                output.Seek(0, SeekOrigin.Begin);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error DecompressSmaugVersion2: \n" + ex, "Decompress", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            finally
            {
                if (inflater != null) inflater.Dispose();
                if (br != null)
                {
                    br.Close();
                    br.Dispose();
                }
            }

            return new BinaryReader(output);
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

            //**

            //ushort num = (ushort)((65280 & flags) >> 8 | (int)(255 & flags) << 8);

            //**
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
