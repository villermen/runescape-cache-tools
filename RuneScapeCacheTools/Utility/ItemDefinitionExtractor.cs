using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public ReferenceTableCache Cache { get; private set; }
        public string? OutputPath { get; }

        public ItemDefinitionExtractor(ReferenceTableCache cache, string? outputPath)
        {
            this.Cache = cache;
            this.OutputPath = outputPath;
        }

        public void ExtractItemDefinitions(string? filter = null, bool skipUndecodableItems = false)
        {
            // Write JSON to string before writing it to file to intercept partial output.
            using var streamWriter = (this.OutputPath != null
                ? new StreamWriter(System.IO.File.Open(this.OutputPath, FileMode.Create))
                : null
            );
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 2,
            };
            var jsonSerializer = new JsonSerializer();

            var itemReferenceTable = this.Cache.GetReferenceTable(CacheIndex.ItemDefinitions);

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(itemReferenceTable.Version.GetValueOrDefault());
            jsonWriter.WritePropertyName("filter");
            jsonWriter.WriteValue(filter);
            jsonWriter.WritePropertyName("items");
            jsonWriter.WriteStartArray();
            // TODO: Commit to file (reused). Needs abstraction.
            streamWriter?.Write(stringWriter.ToString());
            stringWriter.GetStringBuilder().Clear();

            var itemCount = 0;
            var undecodedItemCount = 0;

            var filterParts = filter?.Split(':');

            var serializableProperties = typeof(ItemDefinitionFile).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var fileId in this.Cache.GetAvailableFileIds(CacheIndex.ItemDefinitions))
            {
                var entryFile = this.Cache.GetFile(CacheIndex.ItemDefinitions, fileId);

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

                        var itemJson = stringWriter.ToString();
                        stringWriter.GetStringBuilder().Clear();

                        if (filterParts != null)
                        {
                            var token = (string?)JObject.Parse(itemJson.Trim(',', ' ', '\n')).SelectToken(filterParts[0]);
                            if (token == null)
                            {
                                continue;
                            }
                            if (filterParts.Length == 2 && token.IndexOf(filterParts[1], StringComparison.CurrentCultureIgnoreCase) == -1)
                            {
                                continue;
                            }
                        }

                        if (streamWriter != null)
                        {
                            streamWriter.Write(itemJson);
                        }
                        else
                        {
                            Console.WriteLine(itemJson);
                        }

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
            streamWriter?.Write(stringWriter.ToString());
            stringWriter.GetStringBuilder().Clear();
        }
    }
}
