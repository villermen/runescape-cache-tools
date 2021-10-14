using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Formatting.Json;
using Villermen.RuneScapeCacheTools.Cache;
using Villermen.RuneScapeCacheTools.Exception;
using Villermen.RuneScapeCacheTools.File;
using Villermen.RuneScapeCacheTools.Model;
using System.Text.Json;

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

        public void ExtractItemDefinitions()
        {
            using var jsonWriter = new Utf8JsonWriter(System.IO.File.Open(Path.Combine(this.OutputDirectory, "items.json"), FileMode.Create));
            var itemReferenceTable = this.Cache.GetReferenceTable(CacheIndex.ItemDefinitions);

            jsonWriter.WriteStartObject();
            jsonWriter.WriteNumber("version", itemReferenceTable.Version.GetValueOrDefault());
            jsonWriter.WriteStartArray("items");

            foreach (var fileId in this.Cache.GetAvailableFileIds(CacheIndex.ItemDefinitions))
            {
                var entryFile = this.Cache.GetFile(CacheIndex.ItemDefinitions, fileId);

                jsonWriter.WriteStartObject();

                foreach (var entry in entryFile.Entries.Values)
                {
                    var itemDefinitionFile = ItemDefinitionFile.Decode(entry);

                    foreach (var field in itemDefinitionFile.GetFields())
                    {
                        jsonWriter.WriteString(field.Key, field.Value);
                    }
                }

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();
        }
    }
}
