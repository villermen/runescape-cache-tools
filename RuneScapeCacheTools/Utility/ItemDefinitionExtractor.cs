using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Invalidates item JSON when bumped. Bump whenever output of tool changes.
        /// </summary>
        private const int Protocol = 1;

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

        public void PrintItemDefinitions(string jsonFilePath, string filter)
        {
            throw new NotImplementedException();

            // using var streamReader = new StreamReader(System.IO.File.Open(jsonFilePath, FileMode.Open));
            // using var jsonReader = new JsonTextReader(streamReader);
            //
            // while (jsonReader.Read())
            // {
            // }

            // var filterParts = filter?.Split(':');
            //
            // if (filterParts != null)
            // {
            //     var token = (string?)JObject.Parse(itemJson.Trim(',', ' ', '\n')).SelectToken(filterParts[0]);
            //     if (token == null)
            //     {
            //         continue;
            //     }
            //     if (filterParts.Length == 2 && token.IndexOf(filterParts[1], StringComparison.CurrentCultureIgnoreCase) == -1)
            //     {
            //         continue;
            //     }
            // }
            //
            // if (streamWriter != null)
            // {
            //     streamWriter.Write(itemJson);
            // }
            // else
            // {
            //     Console.WriteLine(itemJson);
            // }
        }
    }
}
