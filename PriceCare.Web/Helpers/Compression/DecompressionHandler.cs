using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace PriceCare.Web.Helpers.Compression
{
    public class DecompressionHandler : HttpClientHandler
    {
        public Collection<ICompressor> Compressors;

        public DecompressionHandler()
        {
            Compressors = new Collection<ICompressor>();
            Compressors.Add(new GZipCompressor());
            //Compressors.Add(new DeflateCompressor());
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.Content.Headers.ContentEncoding != null && response.Content.Headers.ContentEncoding.Any())
            {
                var encoding = response.Content.Headers.ContentEncoding.First();

                var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));

                if (compressor != null)
                {
                    response.Content = await DecompressContent(response.Content, compressor);
                }
            }

            return response;
        }

        private static async Task<HttpContent> DecompressContent(HttpContent compressedContent, ICompressor compressor)
        {
            using (compressedContent)
            {
                MemoryStream decompressed = new MemoryStream();
                await compressor.Decompress(await compressedContent.ReadAsStreamAsync(), decompressed);
                var newContent = new StreamContent(decompressed);
                // copy content type so we know how to load correct formatter
                newContent.Headers.ContentType = compressedContent.Headers.ContentType;

                return newContent;
            }
        }
    }
}