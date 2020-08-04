using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
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
        private const int BlockSize = 102400;

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
        /// Only one processor may be running at a time.
        /// </summary>
        private readonly object _processorLock = new object();

        private TcpClient _contentClient;

        private bool _connected = false;

        private readonly ConcurrentDictionary<Tuple<CacheIndex, int>, TcpFileRequest> _fileRequests = new ConcurrentDictionary<Tuple<CacheIndex, int>, TcpFileRequest>();

        public byte[] DownloadFileData(CacheIndex index, int fileId)
        {
            // Add the request, or get an existing one
            var requestKey = new Tuple<CacheIndex, int>(index, fileId);
            var request = this._fileRequests.GetOrAdd(requestKey, tuple => new TcpFileRequest());

            Task.Run(this.ProcessRequests);

            return request.WaitForCompletionAsync().Result;
        }

        /// <summary>
        /// Requests all files to be requested and processes
        /// </summary>
        private void ProcessRequests()
        {
            // We simply exit when no lock can be obtained instead of queueing additional processors for every request.
            // Race conditions are highly unlikely here because the processor will remain active for a while after all
            // requests are completed.
            if (!Monitor.TryEnter(this._processorLock))
            {
                return;
            }

            try
            {
                if (!this._connected)
                {
                    this.Connect();
                }

                Log.Debug("Starting TCP response processor...");

                if (this._contentClient.Available > 0)
                {
                    throw new DownloaderException(
                        "Started TCP response processor with data already available. Previous one probably didn't fully finish."
                    );
                }

                // Keep running for up to 3 seconds after finishing for new requests to arrive.
                var stopwatch = Stopwatch.StartNew();
                var lastPendingMilliseconds = 0L;
                while (stopwatch.ElapsedMilliseconds < lastPendingMilliseconds + 3000)
                {
                    // Request unrequested files.
                    var pendingFileRequests = 0;
                    foreach (var requestPair in this._fileRequests)
                    {
                        // Limit amount of pending file requests because we can actually overload the content server.
                        pendingFileRequests++;
                        if (pendingFileRequests > 10)
                        {
                            break;
                        }

                        if (requestPair.Value.Requested)
                        {
                            continue;
                        }

                        // Request the file.
                        var writer = new BinaryWriter(this._contentClient.GetStream());

                        writer.Write((byte)(requestPair.Key.Item1 == CacheIndex.ReferenceTables ? 1 : 0));
                        writer.Write((byte)requestPair.Key.Item1);
                        writer.WriteInt32BigEndian(requestPair.Key.Item2);

                        requestPair.Value.MarkRequested(stopwatch.ElapsedMilliseconds);
                    }

                    // Read one block (we want to prioritize sending the requests so we wait less).
                    if (this._contentClient.Available >= 5)
                    {
                        var reader = new BinaryReader(this._contentClient.GetStream());

                        var positionInBlock = 0;

                        // Check which file this chunk is for
                        var index = (CacheIndex)reader.ReadByte();
                        positionInBlock += 1;
                        var awkwardFileId = reader.ReadUInt32BigEndian();
                        positionInBlock += 4;
                        // First bit seems to be a flag that is 0 for reference tables and 1 for regular files.
                        // var regularFile = awkwardFileId >> 31;
                        var fileId = (int)(awkwardFileId & 0x7fffffff);

                        var requestKey = new Tuple<CacheIndex, int>(index, fileId);

                        if (!this._fileRequests.ContainsKey(requestKey))
                        {
                            throw new DownloaderException($"Retrieved data for file {(int)requestKey.Item1}/{requestKey.Item2} which wasn't requested.");
                        }

                        var fileRequest = this._fileRequests[requestKey];

                        // The first part of the file contains the size information. We need to write it but also know
                        // it here to determine the remaining size.
                        if (!fileRequest.MetaWritten)
                        {
                            // This mimicks the logic in RuneTek5CacheFile.
                            var compressionType = (CompressionType)reader.ReadByte();
                            fileRequest.DataWriter.Write((byte)compressionType);
                            positionInBlock += 1;
                            var compressedSize = reader.ReadInt32BigEndian();
                            fileRequest.DataWriter.WriteInt32BigEndian(compressedSize);
                            positionInBlock += 4;
                            if (compressionType != CompressionType.None)
                            {
                                fileRequest.DataWriter.WriteInt32BigEndian(reader.ReadInt32BigEndian());
                                positionInBlock += 4;
                            }

                            fileRequest.RemainingSize = compressedSize;
                            fileRequest.MarkMetaWritten();
                        }

                        var remainingBlockSize = TcpFileDownloader.BlockSize - positionInBlock;

                        // If the file data can not fill the block the block will be smaller.
                        if (remainingBlockSize > fileRequest.RemainingSize)
                        {
                            remainingBlockSize = fileRequest.RemainingSize;
                        }

                        fileRequest.DataWriter.Write(reader.ReadBytesExactly(remainingBlockSize));
                        fileRequest.RemainingSize -= remainingBlockSize;

                        if (fileRequest.RemainingSize <= 0)
                        {
                            // Remove _before_ completing so that we complete a request even if it is added right here.
                            if (!this._fileRequests.TryRemove(requestKey, out var removedRequest))
                            {
                                // Should not be possible because we have a lock.
                                throw new DownloaderException("Could not remove file request.");
                            }

                            removedRequest.MarkCompleted();
                        }
                    }

                    // Handle individual file timeout.
                    foreach (var requestPair in this._fileRequests)
                    {
                        var fileRequest = requestPair.Value;

                        if (fileRequest.RequestedAtMilliseconds < stopwatch.ElapsedMilliseconds - 10000)
                        {
                            fileRequest.MarkFailed(new DownloaderException(
                                $"File request for {(int)requestPair.Key.Item1}/{requestPair.Key.Item2} timed out after 10 seconds."
                            ));

                            if (!this._fileRequests.TryRemove(requestPair.Key, out _))
                            {
                                // Should not be possible because we have a lock.
                                throw new DownloaderException("Could not remove file request.");
                            }
                        }
                    }

                    // Done at end of loop so that server slowness can't break this loop.
                    if (pendingFileRequests > 0)
                    {
                        lastPendingMilliseconds = stopwatch.ElapsedMilliseconds;
                    }
                }

                stopwatch.Stop();

                Log.Debug("TCP request processor finished.");
            }
            catch (System.Exception exception)
            {
                // We blindly launch this task so exceptions are silently discarded. Fail every pending file request
                // with the exception so it will be handled where it matters.
                foreach (var requestPair in this._fileRequests)
                {
                    requestPair.Value.MarkFailed(exception);
                }

                Log.Debug("TCP request processor failed.");
            }
            finally
            {
                Monitor.Exit(this._processorLock);
            }
        }

        private void Connect()
        {
            if (this._connected)
            {
                throw new DownloaderException("Tried to connect while already connected.");
            }

            // Retry connecting with an increasing major version until the server no longer reports we're outdated
            var currentBuildNumber = ClientDetails.GetBuildNumber();
            var connected = false;
            while (!connected)
            {
                this._contentClient = new TcpClient(
                    ClientDetails.GetContentServerHostname(),
                    ClientDetails.GetContentServerTcpPort()
                );

                var handshakeWriter = new BinaryWriter(this._contentClient.GetStream());
                var handshakeReader = new BinaryReader(this._contentClient.GetStream());

                var handshakeKey = ClientDetails.GetContentServerTcpHandshakeKey();

                Log.Debug($"Attempting to connect to TCP content server with version {currentBuildNumber.Item1}.{currentBuildNumber.Item2}...");

                var handshakeLength = (byte) (9 + handshakeKey.Length + 1);

                handshakeWriter.Write(TcpFileDownloader.HandshakeType);
                handshakeWriter.Write(handshakeLength);
                handshakeWriter.WriteInt32BigEndian(currentBuildNumber.Item1);
                handshakeWriter.WriteInt32BigEndian(currentBuildNumber.Item2);
                handshakeWriter.WriteNullTerminatedString(handshakeKey);
                handshakeWriter.Write((byte)Language.English);
                handshakeWriter.Flush();

                var response = (HandshakeResponse)handshakeReader.ReadByte();

                switch (response)
                {
                    case HandshakeResponse.Success:
                        connected = true;
                        ClientDetails.SetBuildNumber(currentBuildNumber);
                        break;

                    case HandshakeResponse.Outdated:
                        this._contentClient.Dispose();
                        this._contentClient = null;
                        currentBuildNumber = new Tuple<int, int>(currentBuildNumber.Item1 + 1, 1);
                        break;

                    default:
                        this._contentClient.Dispose();
                        this._contentClient = null;
                        throw new DownloaderException($"Content server responded to handshake with {response}.");
                }
            }

            Log.Debug($"Successfully connected to content server with version {currentBuildNumber.Item1}.{currentBuildNumber.Item2}.");

            // Required loading element sizes.
            var contentReader = new BinaryReader(this._contentClient.GetStream());
            contentReader.ReadBytesExactly(TcpFileDownloader.LoadingRequirements * 4);

            // Send the initial connection status and login packets to the server. I don't know what the individual
            // writes mean but they do the trick.
            Log.Debug("Sending initial connection status and login packets...");
            var writer = new BinaryWriter(this._contentClient.GetStream());
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
