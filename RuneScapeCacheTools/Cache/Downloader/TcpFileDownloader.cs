using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Villermen.RuneScapeCacheTools.Cache.RuneTek5;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.Cache.Downloader
{
    public class TcpFileDownloader : IDisposable
    {
        /// <summary>
        /// The amount of bytes the content server sends us each time.
        /// </summary>
        private const int BlockLength = 102400;

        /// <summary>
        /// Required to correctly connect to the content server.
        /// </summary>
        private const byte HandshakeType = 15;

        /**
         * No idea what these are for but this is the amount of integers read out directly after connecting to the
         * content server.
         */
        private const int LoadingRequirements = 27;

        /// <summary>
        /// The current build number is needed to correctly connect to the content server. This is not readily available
        /// so we start from a known number and increase it every time the server gives an "outdated" response.
        ///
        /// TODO: Can we be smart about this and skip ahead? Do we get a different response when the major version is too high? Or can we somehow obtain it from another place? The client must know when to update itself doesn't it?
        /// </summary>
        private static readonly Tuple<int, int> StartBuildNumber = new Tuple<int, int>(911, 1);

        /// <summary>
        /// Only one processor may be running at a time.
        /// </summary>
        private readonly object _processorLock = new object();

        private TcpClient _contentClient;

        private bool _connected = false;

        private readonly List<TcpFileRequest> _fileRequests = new List<TcpFileRequest>();

        public byte[] DownloadFileData(CacheIndex index, int fileId)
        {
            // Add the request, or get an existing one
            var request = new TcpFileRequest(index, fileId);
            this._fileRequests.Add(request);

            Task.Run(this.ProcessRequests);

            return request.WaitForCompletionAsync().Result;
        }

        /// <summary>
        /// Requests all files to be requested and processes
        /// </summary>
        /// <exception cref="DownloaderException"></exception>
        private void ProcessRequests()
        {
            lock (this._processorLock)
            {
                // Check if there are any file requests after the lock is obtained.
                if (!this._fileRequests.Any())
                {
                    return;
                }

                if (!this._connected)
                {
                    this.Connect();
                }

                while (this._fileRequests.Any())
                {
                    // Request all unrequested files
                    foreach (var request in this._fileRequests.Where(request => !request.Requested))
                    {
                        var writer = new BinaryWriter(this._contentClient.GetStream());

                        writer.Write((byte)(request.Index == CacheIndex.ReferenceTables ? 1 : 0));
                        writer.Write((byte)request.Index);
                        writer.WriteInt32BigEndian(request.FileId);

                        request.Requested = true;

                        // TODO: Limit to x amount of pending requests?
                    }

                    // Read one chunk
                    if (this._contentClient.Available >= 5)
                    {
                        var reader = new BinaryReader(this._contentClient.GetStream());

                        var readByteCount = 0;

                        // Check which file this chunk is for
                        var index = (CacheIndex)reader.ReadByte();
                        var fileId = reader.ReadInt32BigEndian() & 0x7fffffff; // TODO: ReadUInt32BigEndian()?

                        readByteCount += 5;

                        var requestKey = new Tuple<CacheIndex, int>(index, fileId);

                        var request = this._fileRequests.First(req => req.Index == index && req.FileId == fileId);

                        // The first part of the file always contains the filesize, which we need to know, but is also part of the file
                        if (!request.MetaWritten)
                        {
                            var compressionType = (CompressionType) reader.ReadByte();
                            var length = reader.ReadInt32BigEndian();

                            readByteCount += 5;

                            request.WriteMeta(compressionType, length);
                        }

                        var remainingBlockLength = TcpFileDownloader.BlockLength - readByteCount;

                        if (remainingBlockLength > request.RemainingLength)
                        {
                            remainingBlockLength = request.RemainingLength;
                        }

                        request.WriteContent(reader.ReadBytes(remainingBlockLength));

                        if (request.Completed)
                        {
                            this._fileRequests.Remove(request);
                            this._fileRequests.NotifyYaddaOrSomething();
                        }
                    }
                    else
                    {
                        // We're waiting for bytes to arrive so we're not in a hurry right now.
                        Thread.Sleep(0);
                    }
                }
            }
        }

        private void Connect()
        {
            if (this._connected)
            {
                throw new DownloaderException("Tried to connect while already connected.");
            }

            var key = this.GetKey();

            // Retry connecting with an increasing major version until the server no longer reports we're outdated
            var connected = false;
            while (!connected)
            {
                this._contentClient = new TcpClient(this._contentHost, this._contentPort);

                var handshakeWriter = new BinaryWriter(this._contentClient.GetStream());
                var handshakeReader = new BinaryReader(this._contentClient.GetStream());

                var handshakeLength = (byte) (9 + key.Length + 1);

                handshakeWriter.Write(TcpFileDownloader.HandshakeType);
                handshakeWriter.Write(handshakeLength);
                handshakeWriter.WriteInt32BigEndian(this._contentVersionMajor);
                handshakeWriter.WriteInt32BigEndian(TcpFileDownloader.ContentVersionMinor);
                handshakeWriter.WriteNullTerminatedString(key);
                handshakeWriter.Write((byte)Language.English);
                handshakeWriter.Flush();

                var response = (HandshakeResponse)handshakeReader.ReadByte();

                switch (response)
                {
                    case HandshakeResponse.Success:
                        connected = true;
                        break;

                    case HandshakeResponse.Outdated:
                        this._contentClient.Dispose();
                        this._contentClient = null;
                        this._contentVersionMajor++;
                        break;

                    default:
                        this._contentClient.Dispose();
                        this._contentClient = null;
                        throw new DownloaderException($"Content server responded to handshake with {response}.");
                }
            }

            // Required loading element sizes.
            var contentReader = new BinaryReader(this._contentClient.GetStream());
            contentReader.ReadBytes(TcpFileDownloader.LoadingRequirements * 4);

            // Send the initial connection status and login packets to the server.
            TcpFileDownloader.Logger.Debug("Sending initial connection status and login packets.");

            var writer = new BinaryWriter(this._contentClient.GetStream());

            // I don't know what exactly, but this is how it's done
            writer.Write((byte)6);
            writer.WriteUInt24BigEndian(4);
            writer.WriteInt16BigEndian(0);
            writer.Flush();

            writer.Write((byte)3);
            writer.WriteUInt24BigEndian(0);
            writer.WriteInt16BigEndian(0);
            writer.Flush();

            this._connected = true;
        }

        public void Dispose()
        {
            if (this._contentClient != null)
            {
                this._contentClient.Dispose();
                this._contentClient = null;
            }
        }
    }
}
