using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Helpers.Compression
{
    public class GZipCompressor : Compressor
    {
        private const string GZipEncoding = "gzip";

        public override string EncodingType
        {
            get { return GZipEncoding; }
        }

        public override Stream CreateCompressionStream(Stream output)
        {
            return new GZipStream(output, CompressionMode.Compress);
        }

        public override Stream CreateDecompressionStream(Stream input)
        {
            return new GZipStream(input, CompressionMode.Decompress);
        }
    }
}