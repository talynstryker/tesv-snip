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

        /// <summary>
        /// Compress a ZLib stream
        /// </summary>
        /// <param name="input">stream</param>
        /// <param name="compressLevel">compression level of stream</param>
        /// <returns>compressed stream</returns>
        public static byte[] Compress(Stream input, CompressLevel compressLevel)
        {
            uint maxBytesAddressing = (uint) input.Length;
            Deflater deflater = null;
            MemoryStream output = null;
            BinaryReader br = null;

            try
            {
                deflater = new Deflater(compressLevel);
                output = new MemoryStream();
                br = new BinaryReader(input);

                deflater.DataAvailable += output.Write;
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                while (maxBytesAddressing > 0)
                {
                    uint numBytes = Math.Min(maxBytesAddressing, 8192);
                    deflater.Add(br.ReadBytes((int) numBytes));
                    maxBytesAddressing -= numBytes;
                }

                deflater.Finish(); //flush zlib buffer

                output.Seek(0, SeekOrigin.Begin);

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ErrorWtihNewLine"), ex.ToString()),
                    TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_Compress"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (deflater != null) deflater.Dispose();
                if (br != null)
                {
                    br.Close();
                    br.Dispose();
                }
            }

            if (output != null)
                return output.ToArray();

            MessageBox.Show(
                TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_OutputBufferEmpty"),
                TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_Compress"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }

        /// <summary>
        /// Decompress a ZLib stream
        /// </summary>
        /// <param name="input">stream</param>
        /// <param name="compressLevel">compression level of stream</param>
        /// <param name="expectedSize">size</param>
        /// <returns>decompressed stream</returns>
        public static BinaryReader Decompress(Stream input, out CompressLevel compressLevel, int expectedSize = 0)
        {
            uint numBytesAddressing = (uint) input.Length;
            MemoryStream output = null;
            Inflater inflater = null;
            BinaryReader br = null;
            compressLevel = CompressLevel.None;

            try
            {
                inflater = new Inflater();
                output = new MemoryStream(expectedSize);
                br = new BinaryReader(input);

                br.BaseStream.Seek(0, SeekOrigin.Begin);
                output.Seek(0, SeekOrigin.Begin);

                inflater.DataAvailable += output.Write;

                //br.BaseStream.Seek(0, SeekOrigin.Begin);
                //compressLevel = RetrieveCompressionLevel(br);
                //br.BaseStream.Seek(0, SeekOrigin.Begin);

                while (numBytesAddressing > 0u)
                {
                    uint numBytes = Math.Min(numBytesAddressing, 8192u);
                    inflater.Add(br.ReadBytes((int) numBytes));
                    numBytesAddressing -= numBytes;
                }

                inflater.Finish(); //flush zlib buffer

                output.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ErrorWtihNewLine"), ex.ToString()),
                    TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_Decompress"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (output != null)
                return new BinaryReader(output);
            else
            {
                MessageBox.Show(
                    TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_OutputBufferEmpty"),
                    TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_Decompress"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
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
        ///       FLEVEL (Compression level)
        /// These flags are available for use by specific compression
        /// methods.  The "deflate" method (CM = 8) sets these flags as
        /// follows:
        ///
        ///   0 - compressor used fastest algorithm
        ///   1 - compressor used fast algorithm
        ///   2 - compressor used default algorithm
        ///   3 - compressor used maximum compression, slowest algorithm
        /// 
        ///  ZLib enumeration
        /// 	public enum CompressLevel
        /// 	{
        /// 	Default = -1,
        /// 	None,
        /// 	Best = 9,
        /// 	Fastest = 1
        /// 	}
        /// 
        /// http://code.activestate.com/lists/python-list/204644/
        /// Two bytes, that's not really a lot of known plaintext, but every little bit might be a liability, I guess.
        /// </summary>
        private static CompressLevel RetrieveCompressionLevel(BinaryReader br)
        {
            int cmf = br.ReadBytes(1)[0];
            int flg = br.ReadBytes(1)[0];
            CompressLevel flevel;

            flg = (flg & 0xC0); // Mask : 11000000 = 192 = 0xC0
            flg = flg >> 6;

            switch (flg)
            {
                case 0:
                    flevel = CompressLevel.None;
                    break;
                case 1:
                    flevel = CompressLevel.Fastest;
                    break;
                case 2:
                    flevel = CompressLevel.Default;
                    break;
                case 3:
                    flevel = CompressLevel.Best;
                    break;
                default:
                    flevel = CompressLevel.Best;
                    break;
            }

            return flevel;
        }

    }
}
