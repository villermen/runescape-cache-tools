using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
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
        public string OutputDirectory
        {
            get => this._outputDirectory;
            set => this._outputDirectory = PathExtensions.FixDirectory(value);
        }

        public ReferenceTableCache Cache { get; private set; }

        private string _outputDirectory;

        public ItemDefinitionExtractor(ReferenceTableCache cache, string outputDirectory)
        {
            this.Cache = cache;
            this.OutputDirectory = outputDirectory;
        }

        public void ExtractItemDefinitions(bool skipUndecodableItems = false)
        {
            using var streamWriter = new StreamWriter(System.IO.File.Open(Path.Combine(this.OutputDirectory, "items.json"), FileMode.Create));
            using var jsonWriter = new JsonTextWriter(streamWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 2,
            };
            var jsonSerializer = new JsonSerializer();

            var itemReferenceTable = this.Cache.GetReferenceTable(CacheIndex.ItemDefinitions);

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(itemReferenceTable.Version.GetValueOrDefault());
            jsonWriter.WritePropertyName("items");
            jsonWriter.WriteStartArray();

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
                    }
                    catch (DecodeException exception)
                    {
                        if (!skipUndecodableItems)
                        {
                            throw;
                        }

                        Log.Information($"Could not decode {(int)CacheIndex.ItemDefinitions}/{fileId}/{entry.Key}: {exception.Message}");
                    }
                }
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
        }
    }
}
