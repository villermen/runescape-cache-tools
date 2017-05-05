using System.IO;
using System.Net;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.FileTypes;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Exceptions;
using Villermen.RuneScapeCacheTools.Extensions;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class HttpFileDownloader : IFileDownloader
    {
        private readonly string _baseUrl;

        public HttpFileDownloader(string baseUrl = "http://content.runescape.com")
        {
            this._baseUrl = baseUrl;
        }

        public async Task<BinaryFile> DownloadFileAsync(Index index, int fileId, CacheFileInfo fileInfo)
        {
            var webRequest = WebRequest.CreateHttp($"{this._baseUrl}/ms?m=0&a={(int)index}&g={fileId}&c={fileInfo.Crc}&v={fileInfo.Version}");

            using (var response = (HttpWebResponse)await webRequest.GetResponseAsync())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new DownloaderException($"HTTP interface responded with status code: {response.StatusCode}.");
                }

                if (response.ContentLength != fileInfo.CompressedSize)
                {
                    throw new DownloaderException($"Downloaded file size {response.ContentLength} does not match expected {fileInfo.CompressedSize}.");
                }

                var dataStream = new MemoryStream();
                var dataWriter = new BinaryWriter(dataStream);

                var responseReader = new BinaryReader(response.GetResponseStream());
                dataWriter.Write(responseReader.ReadBytes((int)response.ContentLength));

                // Append version
                dataWriter.WriteUInt16BigEndian((ushort)fileInfo.Version);

                var file = new BinaryFile
                {
                    Info = fileInfo
                };

                file.Decode(dataStream.ToArray());

                return file;
            }
        }
    }
}