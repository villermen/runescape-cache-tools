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
    /// Obtains and caches properties that the client needs to launch the game.
    /// </summary>
    public static class ClientProperties
    {
        /// <summary>
        /// URL of webpage that contains the client properties.
        /// </summary>
        private const string PropertiesUrl = "http://world21.runescape.com/jav_config.ws?binaryType=2";

        private const string ContentServerHostname = "content.runescape.com";

        private const int ContentServerTcpPort = 43594;

        private static readonly Regex PropertyRegex = new Regex(@"^(\w+(?:=\w+)?)=(.*)$", RegexOptions.Multiline);

        private static Dictionary<string, string>? _cachedProperties;

        public static string GetContentServerHostname()
        {
            // TODO: Obtain from params when we figure out how their keys work. Key 37/49 @ 22-2-2021
            return ClientProperties.ContentServerHostname;
        }

        public static int GetContentServerTcpPort()
        {
            // TODO: Obtain from params when we figure out how their keys work. Key 41/43/45/47 @ 22-2-2021
            return ClientProperties.ContentServerTcpPort;
        }

        /// <summary>
        /// Returns a key to connect to the content server using TCP.
        /// </summary>
        /// <returns></returns>
        public static string GetContentServerTcpHandshakeKey()
        {
            // TODO: Could be obtained with params when we figure out their keys. Key 29 @ 22-2-2021
            return ClientProperties.GetParamWithLength(32);
        }

        /// <summary>
        /// Fetches the current server version of the game.
        /// </summary>
        public static Tuple<int, int> GetServerVersion()
        {
            return new Tuple<int, int>(int.Parse(ClientProperties.GetApplicationProperties()["server_version"]), 1);
        }

        /// <summary>
        /// Note that keys change between game versions so they can't be relied on just yet.
        /// <exception cref="ClientPropertiesException"></exception>
        /// </summary>
        public static Dictionary<string, string> GetApplicationProperties()
        {
            if (ClientProperties._cachedProperties != null)
            {
                return ClientProperties._cachedProperties;
            }

            var request = WebRequest.CreateHttp(ClientProperties.PropertiesUrl);
            try
            {
                using var response = (HttpWebResponse)request.GetResponse();
                var responseHtml = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var matches = ClientProperties.PropertyRegex.Matches(responseHtml);
                if (matches.Count == 0)
                {
                    throw new ClientPropertiesException("No properties could be obtained from the property page.");
                }

                ClientProperties._cachedProperties = matches
                    .Cast<Match>()
                    .ToDictionary(
                        match => match.Groups[1].Value.Contains("=") ? match.Groups[1].Value.Replace('=', '[') + "]" : match.Groups[1].Value,
                        match => match.Groups[2].Value
                    );

                return ClientProperties._cachedProperties;
            }
            catch (WebException exception)
            {
                throw new ClientPropertiesException(
                    "Could not obtain client properties because the request failed.",
                    exception
                );
            }
        }

        /// <summary>
        /// Returns a client parameter by value length.
        /// </summary>
        /// <exception cref="ClientPropertiesException"></exception>
        private static string GetParamWithLength(int length)
        {
            var properties = ClientProperties.GetApplicationProperties();
            var matchingParams = properties.Where(property =>
                property.Key.StartsWith("param[") && property.Value.Length == length
            ).ToArray();

            if (matchingParams.Length == 1)
            {
                return matchingParams[0].Value;
            }

            throw new ClientPropertiesException(matchingParams.Length == 0
                ? $"No parameters with a length of \"{length}\" could be found."
                : $"Multiple parameters with a length of \"{length}\" were found."
            );
        }
    }
}
