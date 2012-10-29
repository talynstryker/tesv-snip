namespace TESVSnip.Domain.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    using TESVSnip.DotZLib;

    public static class ZLib
    {
        private static Deflater deflater;

        private static Inflater inflater;

        public static string Version { get; private set; }

        [Obsolete("Compress(byte[] input)")]
        public static byte[] Compress(Stream input)
        {
            // if (input == null)
            // {
            // throw new ArgumentNullException("input");
            // }

            // var buffer = new byte[input.Length];
            // input.Read(buffer, 0, (int)input.Length);

            // return Compress(buffer);
            return null;
        }

        public static byte[] Compress(byte[] input)
        {
            using (var output = new MemoryStream())
            {
                using (var deflater1 = new Deflater(CompressLevel.Best))
                {
                    deflater1.DataAvailable += output.Write;
                    deflater1.Add(input, 0, input.Length);
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
            var maxBytesAddressing = (uint)input.Length;
            byte[] returnedArrayOfBytes;

            Deflater deflater1 = new Deflater(compressLevel);
            MemoryStream output = new MemoryStream();
            input.Seek(0, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(input);

            try
            {
                deflater1.DataAvailable += output.Write;
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                while (maxBytesAddressing > 0)
                {
                    uint numBytes = Math.Min(maxBytesAddressing, 8192);
                    deflater1.Add(br.ReadBytes((int)numBytes));
                    maxBytesAddressing -= numBytes;
                }

                deflater1.Finish(); // flush zlib buffer

                // Line for debug and test
                // output.Seek(0, SeekOrigin.Begin);
                // TESVSnip.Domain.Services.SaveReadStream.SaveStreamToDisk(output);
                output.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ErrorWithNewLine"), ex.ToString()),
                    TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_Compress"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                returnedArrayOfBytes = output.ToArray();

                deflater1.Dispose();

                br.Close();
                br.Dispose();
            }

            Debug.Assert(returnedArrayOfBytes != null, "returnedArrayOfBytes != null");
            return returnedArrayOfBytes;

            //MessageBox.Show(
            //    TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_OutputBufferEmpty"),
            //    TranslateUI.TranslateUIGlobalization.RM.GetString("MSG_ZLib_Compress"),
            //    MessageBoxButtons.OK, MessageBoxIcon.Error);
            //return null;
        }

        /// <summary>
        /// Decompress a ZLib stream
        /// </summary>
        /// <param name="compressLevel">
        /// compression level of stream
        /// </param>
        /// <param name="expectedSize">
        /// size
        /// </param>
        public static void Decompress(out CompressLevel compressLevel, int expectedSize = 0)
        {
            uint numBytesAddressing = ZLibWrapper.InputBufferLength;
            //MemoryStream output = null;

            //BinaryReader br = null;
            compressLevel = CompressLevel.None;

            try
            {
                ZLibWrapper.Position(0, ZLibBufferType.InputBuffer);
                compressLevel = RetrieveCompressionLevel();
                ZLibWrapper.Position(0, ZLibBufferType.InputBuffer);
                //ReadBytes(  ) br.BaseStream.Seek(0, SeekOrigin.Begin);

                if (inflater == null)
                {
                    inflater = new Inflater();
                    inflater.DataAvailable += ZLibWrapper.WriteInOutputBuffer;
                }

                //output = new MemoryStream(expectedSize);
                //br = new BinaryReader(input);
                //br.BaseStream.Seek(0, SeekOrigin.Begin);
                //output.Seek(0, SeekOrigin.Begin);

                //inflater.DataAvailable += delegate(byte[] data, int position, int count)
                //{
                //    ZLibStreamWrapper.WriteInOutputBuffer(data, position, count);
                //};DataAvailableHandler

                //inflater.DataAvailable += ZLibStreamWrapper.WriteInOutputBuffer;


                //br.BaseStream.Seek(0, SeekOrigin.Begin);
                //compressLevel = RetrieveCompressionLevel(br);
                //br.BaseStream.Seek(0, SeekOrigin.Begin);

                while (numBytesAddressing > 0u)
                {
                    uint numBytes = Math.Min(numBytesAddressing, 8192u); //8192u); 65536u
                    inflater.Add(ZLibWrapper.ReadBytes((int)numBytes, ZLibBufferType.InputBuffer));
                    //inflater.Add(br.ReadBytes((int) numBytes));   
                    numBytesAddressing -= numBytes;
                }

                if (!string.IsNullOrWhiteSpace(inflater._ztream.msg))
                    MessageBox.Show(
                        string.Format("ZLib.Decompress: {0}", inflater._ztream.msg),
                        @"ZLib Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                inflater.Finish(); //flush zlib buffer

                ZLibWrapper.Position(0, ZLibBufferType.OutputBuffer); //output.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ErrorWithNewLine"),
                                  ex.ToString()),
                    TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_Decompress"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (ZLibWrapper.OutputBufferLength == 0)
                {
                    MessageBox.Show(
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_OutputBufferEmpty"),
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_Decompress"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static void Initialize()
        {
            Version = Info.Version;
        }

        public static void ReleaseDeflater()
        {
            if (deflater != null)
            {
                deflater.DataAvailable -= ZLibWrapper.WriteInOutputBuffer;
                deflater.Dispose();
                deflater = null;
            }
        }

        public static void ReleaseInflater()
        {
            if (inflater != null)
            {
                inflater.DataAvailable -= ZLibWrapper.WriteInOutputBuffer;
                inflater.Dispose();
                inflater = null;
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
        private static CompressLevel RetrieveCompressionLevel()
        {
            byte cmf = ZLibWrapper.ReadBytes(1, ZLibBufferType.InputBuffer)[0];
            int flg = ZLibWrapper.ReadBytes(1, ZLibBufferType.InputBuffer)[0];
            CompressLevel fLevel;

            flg = (flg & 0xC0); // Mask : 11000000 = 192 = 0xC0
            flg = flg >> 6;

            switch (flg)
            {
                case 0:
                    fLevel = CompressLevel.None;
                    break;
                case 1:
                    fLevel = CompressLevel.Fastest;
                    break;
                case 2:
                    fLevel = CompressLevel.Default;
                    break;
                case 3:
                    fLevel = CompressLevel.Best;
                    break;
                default:
                    fLevel = CompressLevel.Best;
                    break;
            }

            return fLevel;
        }

    }
}
