using System;
using System.IO;
using System.Net;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class HttpFileDownloader
    {
        public byte[] DownloadFileData(CacheIndex index, int fileId, CacheFileInfo fileInfo)
        {
            if (!fileInfo.Crc.HasValue)
            {
                throw new ArgumentException("File CRC must be set when requesting HTTP files.");
            }

            if (!fileInfo.Version.HasValue)
            {
                throw new ArgumentException("File version must be set when requesting HTTP files.");
            }

            if (!fileInfo.CompressedSize.HasValue)
            {
                throw new ArgumentException("File compressed size must be set when requesting HTTP files.");
            }

            var webRequest = WebRequest.CreateHttp(
                $"https://{ClientProperties.GetContentServerHostname()}/ms?m=0&a={(int)index}&k={ClientProperties.GetServerVersion().Item1}&g={fileId}&c={fileInfo.Crc}&v={fileInfo.Version}"
            );

            try
            {
                using var response = (HttpWebResponse)webRequest.GetResponse();
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
                dataWriter.Write(responseReader.ReadBytesExactly((int)response.ContentLength));

                return dataStream.ToArray();
            }
            catch (WebException exception)
            {
                throw new DownloaderException(
                    $"Could not download {(int)index}/{fileId} via HTTP due to a request error.",
                    exception
                );
            }
        }
    }
}
