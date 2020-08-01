using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Villermen.RuneScapeCacheTools.Cache.Downloader;
using Villermen.RuneScapeCacheTools.Exception;

namespace Villermen.RuneScapeCacheTools.Utility
{
    /// <summary>
    /// Obtains basic details about the current version of the client.
    /// </summary>
    public static class ClientDetails
    {
        /// <summary>
        /// URL of webpage that contains the client &lt;applet&gt; element together with all the parameter that we'll
        /// need.
        /// </summary>
        private const string AppletUrl = "https://world21.runescape.com/j0";
        // TODO: Use https://world21.runescape.com/jav_config.ws for this.

        private const string ContentServerHostname = "content.runescape.com";

        private const int ContentServerTcpPort = 43594;

        private static readonly Regex AppletParamRegex = new Regex(@"<param name=""([^""]+)"" value=""([^""]+)"">");

        private static Dictionary<string, string>? _cachedAppletParams;

        private static Tuple<int, int> _buildNumber = new Tuple<int, int>(913, 1);

        public static string GetContentServerHostname()
        {
            // TODO: Obtain from params when we figure out how their keys work.
            return ClientDetails.ContentServerHostname;
        }

        public static int GetContentServerTcpPort()
        {
            // TODO: Obtain from params when we figure out how their keys work.
            return ClientDetails.ContentServerTcpPort;
        }

        /// <summary>
        /// Returns a key to connect to the content server using TCP.
        /// </summary>
        /// <returns></returns>
        public static string GetContentServerTcpHandshakeKey()
        {
            // Could be obtained with params when we figure out their keys.
            return ClientDetails.GetAppletParamWithLength(32);
        }

        /// <summary>
        /// Fetches the current build number of the game client. Will most likely return an outdated build number unless
        /// <see cref="TcpFileDownloader" /> has successfully connected.
        ///
        /// TODO: Figure out a way to retrieve just the build number in an efficient way
        /// </summary>
        public static Tuple<int, int> GetBuildNumber()
        {
            return ClientDetails._buildNumber;
        }

        /// <summary>
        /// Build number must be set from outside this class for now.
        /// </summary>
        public static void SetBuildNumber(Tuple<int, int> buildNumber)
        {
            ClientDetails._buildNumber = buildNumber;
        }

        /// <summary>
        /// Returns a set of (cached) applet params obtained by visiting the game page. Note that keys change when the
        /// game is updated, so can't be relied on just yet.
        /// <exception cref="ClientDetailsException"></exception>
        /// </summary>
        public static Dictionary<string, string> GetAppletParams()
        {
            if (ClientDetails._cachedAppletParams != null)
            {
                return ClientDetails._cachedAppletParams;
            }

            var request = WebRequest.CreateHttp(ClientDetails.AppletUrl);
            try
            {
                using var response = (HttpWebResponse)request.GetResponse();
                var responseHtml = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var matches = ClientDetails.AppletParamRegex.Matches(responseHtml);
                if (matches.Count == 0)
                {
                    throw new ClientDetailsException("No params could be obtained from applet page.");
                }

                ClientDetails._cachedAppletParams = matches
                    .Cast<Match>()
                    .ToDictionary(
                        match => match.Groups[1].Value,
                        match => match.Groups[2].Value
                    );

                return ClientDetails._cachedAppletParams;
            }
            catch (WebException exception)
            {
                throw new ClientDetailsException(
                    "Could not obtain applet params because the request failed.",
                    exception
                );
            }
        }

        /// <summary>
        /// Returns an applet param by value length.
        /// </summary>
        /// <exception cref="ClientDetailsException"></exception>
        private static string GetAppletParamWithLength(int length)
        {
            var appletParams = ClientDetails.GetAppletParams();
            var matchingParams = appletParams.Where(appletParam => appletParam.Value.Length == length).ToArray();

            if (matchingParams.Length == 1)
            {
                return matchingParams[0].Value;
            }

            throw new ClientDetailsException(matchingParams.Length == 0
                ? $"No applet param with a length of \"{length}\" could be found."
                : $"Multiple applet params with a length of \"{length}\" were found."
            );
        }
    }
}
