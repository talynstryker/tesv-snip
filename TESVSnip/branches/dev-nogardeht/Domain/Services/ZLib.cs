namespace TESVSnip.Domain.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    using TESVSnip.DotZLib;

    public static class ZLib
    {
        private static Deflater _deflaterBest;
        private static Deflater _deflaterNone;
        private static Deflater _deflaterFastest;
        private static Deflater _deflaterDefault;

        private static Inflater _inflater;

        public static string Version { get; private set; }

        //[Obsolete("Compress(byte[] input)")]
        //public static byte[] Compress(Stream input)
        //{
        //    // if (input == null)
        //    // {
        //    // throw new ArgumentNullException("input");
        //    // }

        //    // var buffer = new byte[input.Length];
        //    // input.Read(buffer, 0, (int)input.Length);

        //    // return Compress(buffer);
        //    return null;
        //}

        //public static byte[] Compress(byte[] input)
        //{
        //    using (var output = new MemoryStream())
        //    {
        //        using (var deflater1 = new Deflater(CompressLevel.Best))
        //        {
        //            deflater1.DataAvailable += output.Write;
        //            deflater1.Add(input, 0, input.Length);
        //        }

        //        return output.ToArray();
        //    }
        //}

        /// <summary>
        /// Compress a ZLib stream
        /// </summary>
        /// <param name="buffer">The buffer. </param>
        /// <param name="compressLevel">Compression level of stream. </param>
        public static void Compress(SnipStreamWrapper snipStreamWrapper, CompressLevel compressLevel)
        {
            var numBytesAddressing = snipStreamWrapper.OutputBufferLength;
            byte[] returnedArrayOfBytes;

            ZLibWrapper.CopyByteArrayToInputBuffer(snipStreamWrapper.OutputBuffer, 0,
                                                   snipStreamWrapper.OutputBufferLength);

            if (_deflaterBest == null)
            {
                _deflaterBest = new Deflater(CompressLevel.Best);
                _deflaterBest.DataAvailable += ZLibWrapper.WriteInOutputBuffer;
            }

            if (_deflaterNone == null)
            {
                _deflaterNone = new Deflater(CompressLevel.None);
                _deflaterNone.DataAvailable += ZLibWrapper.WriteInOutputBuffer;
            }

            if (_deflaterFastest == null)
            {
                _deflaterFastest = new Deflater(CompressLevel.Fastest);
                _deflaterFastest.DataAvailable += ZLibWrapper.WriteInOutputBuffer;
            }

            if (_deflaterDefault == null)
            {
                _deflaterDefault = new Deflater(CompressLevel.Default);
                _deflaterDefault.DataAvailable += ZLibWrapper.WriteInOutputBuffer;
            }

           //MemoryStream output = new MemoryStream();
            //input.Seek(0, SeekOrigin.Begin);
            //BinaryReader br = new BinaryReader(input);

            try
            {
                //_deflater.DataAvailable += output.Write;
                //br.BaseStream.Seek(0, SeekOrigin.Begin);

                //while (maxBytesAddressing > 0)
                //{
                //    uint numBytes = Math.Min(maxBytesAddressing, 8192);
                //    deflater1.Add(br.ReadBytes((int)numBytes));
                //    maxBytesAddressing -= numBytes;
                //}

                while (numBytesAddressing > 0u)
                {
                    uint numBytes = Math.Min(numBytesAddressing, 8192u); //8192u); 65536u

                    if (compressLevel == CompressLevel.None)
                        _deflaterNone.Add(ZLibWrapper.ReadBytes((int)numBytes, ZLibBufferType.InputBuffer));

                    if (compressLevel == CompressLevel.Best)
                        _deflaterBest.Add(ZLibWrapper.ReadBytes((int)numBytes, ZLibBufferType.InputBuffer));

                    if (compressLevel == CompressLevel.Default)
                        _deflaterDefault.Add(ZLibWrapper.ReadBytes((int)numBytes, ZLibBufferType.InputBuffer));

                    if (compressLevel == CompressLevel.Fastest)
                        _deflaterFastest.Add(ZLibWrapper.ReadBytes((int)numBytes, ZLibBufferType.InputBuffer));

                    numBytesAddressing -= numBytes;
                }

                string deflateErrorMsg = string.Empty;
                if (compressLevel == CompressLevel.None)
                    deflateErrorMsg = _deflaterNone._ztream.msg;

                if (compressLevel == CompressLevel.Best)
                    deflateErrorMsg = _deflaterBest._ztream.msg;

                if (compressLevel == CompressLevel.Default)
                    deflateErrorMsg = _deflaterDefault._ztream.msg;

                if (compressLevel == CompressLevel.Fastest)
                    deflateErrorMsg = _deflaterFastest._ztream.msg;

                if (!string.IsNullOrWhiteSpace(deflateErrorMsg))
                    MessageBox.Show(string.Format("ZLib.Compress: {0}", deflateErrorMsg), @"ZLib Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (compressLevel == CompressLevel.None)
                    _deflaterNone.Finish(); // flush zlib buffer    

                if (compressLevel == CompressLevel.Best)
                    _deflaterBest.Finish(); // flush zlib buffer  

                if (compressLevel == CompressLevel.Default)
                    _deflaterDefault.Finish(); // flush zlib buffer      

                if (compressLevel == CompressLevel.Fastest)
                    _deflaterFastest.Finish(); // flush zlib buffer      
     
                // Line for debug and test
                // output.Seek(0, SeekOrigin.Begin);
                // TESVSnip.Domain.Services.SaveReadStream.SaveStreamToDisk(output);
                //output.Seek(0, SeekOrigin.Begin);
                ZLibWrapper.Position(0, ZLibBufferType.OutputBuffer); //output.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ErrorWithNewLine"),
                        ex.ToString()),
                    TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_Decompress"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (ZLibWrapper.OutputBufferLength == 0)
                {
                    MessageBox.Show(
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_OutputBufferEmpty"),
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_Decompress"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show(
            //        string.Format(
            //            TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ErrorWithNewLine"), ex.ToString()),
            //        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ZLib_Compress"),
            //        MessageBoxButtons.OK,
            //        MessageBoxIcon.Error);
            //}
            //finally
            //{
            //    returnedArrayOfBytes = output.ToArray();

            //    deflater1.Dispose();

            //    br.Close();
            //    br.Dispose();
            //}

            //Debug.Assert(returnedArrayOfBytes != null, "returnedArrayOfBytes != null");
            //return returnedArrayOfBytes;

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

                if (_inflater == null)
                {
                    _inflater = new Inflater();
                    _inflater.DataAvailable += ZLibWrapper.WriteInOutputBuffer;
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
                    _inflater.Add(ZLibWrapper.ReadBytes((int)numBytes, ZLibBufferType.InputBuffer));
                    //inflater.Add(br.ReadBytes((int) numBytes));   
                    numBytesAddressing -= numBytes;
                }

                if (!string.IsNullOrWhiteSpace(_inflater._ztream.msg))
                    MessageBox.Show(
                        string.Format("ZLib.Decompress: {0}", _inflater._ztream.msg),
                        @"ZLib Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                _inflater.Finish(); //flush zlib buffer

                ZLibWrapper.Position(0, ZLibBufferType.OutputBuffer); //output.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                        TranslateUI.TranslateUiGlobalization.ResManager.GetString(name: "MSG_ErrorWithNewLine"),
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
            if (_deflaterBest != null)
            {
                _deflaterBest.DataAvailable -= ZLibWrapper.WriteInOutputBuffer;
                _deflaterBest.Dispose();
                _deflaterBest = null;
            }

            if (_deflaterNone != null)
            {
                _deflaterNone.DataAvailable -= ZLibWrapper.WriteInOutputBuffer;
                _deflaterNone.Dispose();
                _deflaterNone = null;
            }

            if (_deflaterFastest != null)
            {
                _deflaterFastest.DataAvailable -= ZLibWrapper.WriteInOutputBuffer;
                _deflaterFastest.Dispose();
                _deflaterFastest = null;
            }

            if (_deflaterDefault != null)
            {
                _deflaterDefault.DataAvailable -= ZLibWrapper.WriteInOutputBuffer;
                _deflaterDefault.Dispose();
                _deflaterDefault = null;
            }
        }

        public static void ReleaseInflater()
        {
            if (_inflater != null)
            {
                _inflater.DataAvailable -= ZLibWrapper.WriteInOutputBuffer;
                _inflater.Dispose();
                _inflater = null;
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
