using System.IO;
using System.Net;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Extension;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
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

            try
            {
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
            catch (WebException exception)
            {
                throw new DownloaderException(
                    $"Could not download {(int)index}/{fileId} due to a request error.",
                    exception
                );
            }
        }
    }
}
