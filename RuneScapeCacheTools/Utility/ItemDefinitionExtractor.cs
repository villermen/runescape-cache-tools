using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;

namespace Villermen.RuneScapeCacheTools.Utility
{
    /// <summary>
    /// Allows extraction of item information in JSON format.
    /// </summary>
    public class ItemDefinitionExtractor
    {
        /// <summary>
        /// Invalidates item JSON when bumped. Bump whenever output of tool (e.g., property names) changes.
        /// </summary>
        private const int Protocol = 3;

        /// <summary>
        /// Returns whether the JSON file's version matches the version in cache. Means extraction can be skipped.
        /// </summary>
        public bool JsonMatchesCache(ReferenceTableCache cache, string jsonFilePath)
        {
            try
            {
                using var streamReader = new StreamReader(System.IO.File.Open(jsonFilePath, FileMode.Open));
                using var jsonReader = new JsonTextReader(streamReader);

                int? jsonVersion = null;
                int? jsonProtocol = null;
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Path == "version")
                    {
                        jsonVersion = jsonReader.ReadAsInt32();
                    }

                    if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Path == "protocol")
                    {
                        jsonProtocol = jsonReader.ReadAsInt32();
                    }

                    if (jsonVersion != null && jsonProtocol != null)
                    {
                        break;
                    }
                }

                if (jsonVersion == null || jsonProtocol != ItemDefinitionExtractor.Protocol)
                {
                    return false;
                }

                var itemReferenceTable = cache.GetReferenceTable(CacheIndex.ItemDefinitions);
                var cacheVersion = itemReferenceTable.Version.GetValueOrDefault();

                return (jsonVersion == cacheVersion);
            }
            catch (FileNotFoundException exception)
            {
                return false;
            }
        }

        public void ExtractItemDefinitions(ReferenceTableCache cache, string jsonFilePath, bool skipUndecodableItems = false)
        {
            // Courtesy backup.
            if (System.IO.File.Exists(jsonFilePath))
            {
                Log.Information("Backing up existing items JSON file...");
                var backupFilepath = jsonFilePath + ".bak";
                System.IO.File.Delete(backupFilepath);
                System.IO.File.Move(jsonFilePath, backupFilepath);
            }

            // Write JSON to string before writing it to file to intercept partial output.
            using var streamWriter = new StreamWriter(System.IO.File.Open(jsonFilePath, FileMode.Create));
            using var jsonWriter = new JsonTextWriter(streamWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 2,
            };
            var jsonSerializer = new JsonSerializer();

            Log.Information("Downloading and writing items JSON...");

            var itemReferenceTable = cache.GetReferenceTable(CacheIndex.ItemDefinitions);

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(itemReferenceTable.Version.GetValueOrDefault());
            jsonWriter.WritePropertyName("protocol");
            jsonWriter.WriteValue(ItemDefinitionExtractor.Protocol);
            jsonWriter.WritePropertyName("items");
            jsonWriter.WriteStartArray();

            var itemCount = 0;
            var undecodedItemCount = 0;

            var serializableProperties = typeof(ItemDefinitionFile).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var fileId in cache.GetAvailableFileIds(CacheIndex.ItemDefinitions))
            {
                var entryFile = cache.GetFile(CacheIndex.ItemDefinitions, fileId);

                foreach (var entry in entryFile.Entries)
                {
                    try
                    {
                        var itemDefinitionFile = ItemDefinitionFile.Decode(entry.Value);
                        // TODO: An abstraction could be made to turn file ID into file + entry and vice versa.
                        itemDefinitionFile.Id = fileId * 256 + entry.Key;

                        jsonWriter.WriteStartObject();
                        foreach (var property in serializableProperties)
                        {
                            var propertyName = Formatter.StringToLowerCamelCase(property.Name);
                            var propertyValue = property.GetValue(itemDefinitionFile);
                            if (propertyValue == null)
                            {
                                continue;
                            }

                            // camelCase item properties and prefix with "unknown" when undefined.
                            if (propertyName == "properties")
                            {
                                propertyValue = ((Dictionary<ItemProperty, object>)propertyValue)
                                    .ToDictionary(
                                        itemProperty => Formatter.StringToLowerCamelCase(Enum.GetName(typeof(ItemProperty), itemProperty.Key) ?? $"unknown{(int)itemProperty.Key}"),
                                        itemProperty => itemProperty.Value
                                    );
                            }

                            // I tried using JsonSerializer for the entire file. Overriding individual properties
                            // requires way to much boilerplate and changes on the original model.
                            jsonWriter.WritePropertyName(propertyName);
                            jsonSerializer.Serialize(jsonWriter, propertyValue);
                        }
                        jsonWriter.WriteEndObject();

                        itemCount++;
                    }
                    catch (DecodeException exception)
                    {
                        if (!skipUndecodableItems)
                        {
                            throw;
                        }

                        Log.Information($"Could not decode {(int)CacheIndex.ItemDefinitions}/{fileId}/{entry.Key}: {exception.Message}");
                        undecodedItemCount++;
                    }
                }
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WritePropertyName("itemCount");
            jsonWriter.WriteValue(itemCount);
            jsonWriter.WritePropertyName("undecodedItemCount");
            jsonWriter.WriteValue(undecodedItemCount);
            jsonWriter.WriteEndObject();
        }

        public void PrintItemDefinitions(string jsonFilePath, string filter, TextWriter output)
        {
            // Note: Method purposefully doesn't log anything so output can be piped in print-only mode.
            var itemFilter = this.CreateItemFilter(filter);

            using var streamReader = new StreamReader(System.IO.File.Open(jsonFilePath, FileMode.Open));
            using var jsonReader = new JsonTextReader(streamReader);

            // Read to first item start token.
            var itemArrayFound = false;
            while (jsonReader.Read())
            {
                if (jsonReader.Path == "items" && jsonReader.TokenType == JsonToken.StartArray)
                {
                    itemArrayFound = true;
                    break;
                }
            }
            if (!itemArrayFound)
            {
                throw new InvalidOperationException("Specified JSON has an unexpected format.");
            }

            using var jsonWriter = new JsonTextWriter(output)
            {
                Formatting = Formatting.Indented,
                Indentation = 2,
            };
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("filter");
            jsonWriter.WriteValue(filter);
            jsonWriter.WritePropertyName("items");
            jsonWriter.WriteStartArray();

            var totalItemCount = 0;
            var itemCount = 0;
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType != JsonToken.StartObject)
                {
                    break;
                }

                var itemObject = JObject.Load(jsonReader);
                totalItemCount++;

                if (!itemFilter(itemObject))
                {
                    continue;
                }

                itemCount++;
                itemObject.WriteTo(jsonWriter);
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WritePropertyName("itemCount");
            jsonWriter.WriteValue(itemCount);
            jsonWriter.WritePropertyName("totalItemCount");
            jsonWriter.WriteValue(totalItemCount);
            jsonWriter.WriteEndObject();
        }

        private Func<JObject, bool> CreateItemFilter(string filter)
        {
            // Parse and validate filters ahead of time.
            var itemFilterLookup = filter
                .Split(',')
                .Select(itemFilter =>
                {
                    var filterParts = itemFilter.Split(':');
                    if (filterParts.Length < 1 || filterParts.Length > 2)
                    {
                        throw new ArgumentException($"Invalid item filter \"{itemFilter}\".");
                    }

                    return filterParts;
                })
                .ToLookup<string[], string, Func<JToken?, bool>>(
                    filterParts =>
                    {
                        if (string.IsNullOrWhiteSpace(filterParts[0]))
                        {
                            throw new ArgumentException($"Invalid item filter path \"{filterParts[0]}\"");
                        }

                        return filterParts[0];
                    },
                    filterParts =>
                    {
                        // Only verify that path exists on the item.
                        if (filterParts.Length == 1)
                        {
                            return (token) => token != null;
                        }

                        var filterValue = filterParts[1];

                        // Verify that path _doesn't_ exist on the item (null).
                        if (filterValue == "~")
                        {
                            return (token) => token == null;
                        }

                        // Verify numeric greater/less than.
                        if (filterValue.StartsWith(">") || filterValue.StartsWith("<"))
                        {
                            if (!int.TryParse(filterValue.Substring(1), out var intFilterValue))
                            {
                                throw new ArgumentException("Filters > and < require an integer value.");
                            }

                            return (token) =>
                            {
                                if (token == null || !int.TryParse(token.ToString(), out var tokenValue))
                                {
                                    return false;
                                }

                                return (filterValue.StartsWith(">")
                                    ? tokenValue > intFilterValue
                                    : tokenValue < intFilterValue
                                );
                            };
                        }

                        // Verify string match with wildcard support.
                        var regex = new Regex(
                            $"^{Regex.Escape(filterValue).Replace("\\*", ".*")}$",
                            RegexOptions.IgnoreCase
                        );
                        return (token) => token != null && regex.IsMatch(token.ToString());
                    }
                );

            return (JObject itemObject) =>
            {
                foreach (var grouping in itemFilterLookup)
                {
                    var token = itemObject.SelectToken(grouping.Key);

                    foreach (var itemFilter in grouping)
                    {
                        if (!itemFilter(token))
                        {
                            return false;
                        }
                    }
                }

                return true;
            };
        }
    }
}
