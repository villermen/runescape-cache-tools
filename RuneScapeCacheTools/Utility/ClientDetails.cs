using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
        private const string AppletUrl = "http://world2.runescape.com/j0";

        private static readonly Regex AppletParamRegex = new Regex(@"<param name=""([^""]+)"" value=""([^""]+)"">");

        private static Dictionary<string, string>? _cachedAppletParams;

        private static Tuple<int, int>? _cachedBuildNumber;

        /// <summary>
        /// Returns a set of (cached) applet params obtained by visiting the game page.
        /// </summary>
        private static Dictionary<string, string> GetAppletParams()
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
        /// Returns an applet param by key.
        /// </summary>
        /// <exception cref="ClientDetailsException"></exception>
        public static string GetAppletParam(string key)
        {
            var appletParams = ClientDetails.GetAppletParams();
            if (!appletParams.ContainsKey(key))
            {
                throw new ClientDetailsException($"Param with key \"{key}\" was not obtained.");
            }

            return appletParams[key];
        }

        public static string GetContentServerHostname()
        {
            var hostname = ClientDetails.GetAppletParam("8");
            if (string.IsNullOrWhiteSpace(hostname))
            {
                throw new ClientDetailsException("Content hostname server parameter is empty.");
            }

            return hostname;
        }

        public static int GetContentServerPort()
        {
            if (!int.TryParse(ClientDetails.GetAppletParam("18"), out var port) || port == 0)
            {
                throw new ClientDetailsException("Could not parse content server port.");
            }

            return port;
        }

        public static string GetContentServerHandshakeKey()
        {
            var handshakeKey = ClientDetails.GetAppletParam("19");
            if (handshakeKey.Length != 32)
            {
                throw new ClientDetailsException("Obtained content server handshake key is invalid.");
            }

            return handshakeKey;
        }

        /// <summary>
        /// Fetches the current build number of the game client.
        /// </summary>
        public static Tuple<int, int> GetBuildNumber()
        {
            if (ClientDetails._cachedBuildNumber != null)
            {
                return ClientDetails._cachedBuildNumber;
            }

            throw new NotImplementedException(
                "A way to fetch just the client's build number has not yet been implemented."
            );
        }

        /// <summary>
        /// Build number must be set from outside this class until I find a way to efficiently retrieve it in a
        /// different way.
        /// </summary>
        public static void SetBuildNumber(Tuple<int, int> buildNumber)
        {
            ClientDetails._cachedBuildNumber = buildNumber;
        }
    }
}
