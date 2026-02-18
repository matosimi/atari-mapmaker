using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace AtariMapMaker
{
    /// <summary>Ensures System.Drawing.Size serializes/deserializes correctly in ElementLibrary and elsewhere.</summary>
    internal class SizeJsonConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int w = 0, h = 0;
            if (reader.TokenType != JsonTokenType.StartObject)
                return new Size(0, 0);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                string prop = reader.GetString();
                reader.Read();
                if (string.Equals(prop, "Width", StringComparison.OrdinalIgnoreCase))
                    w = reader.GetInt32();
                else if (string.Equals(prop, "Height", StringComparison.OrdinalIgnoreCase))
                    h = reader.GetInt32();
            }
            return new Size(w, h);
        }

        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Width", value.Width);
            writer.WriteNumber("Height", value.Height);
            writer.WriteEndObject();
        }
    }

    internal static class AtariJson
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            Converters = { new SizeJsonConverter() }
        };

        public class Atrmap
        {
            // Existing fields (unchanged for backwards compatibility)
            public string AtrmapVersion { get; set; }
            public int[] MapData { get; set; }
            public int[] FontData { get; set; }  // Kept for compatibility
            public int[] Color5 { get; set; }
            public Size MapSize { get; set; }
            public Size MapScreenSize { get; set; }
            public int[] DliData { get; set; }
            
            // New optional fields (v2.0+)
            public int[][] FontDataArray { get; set; }  // Multiple fonts
            public string[] FontFileNames { get; set; }  // Font file names
            public int[] FontLineMappingPerScreen { get; set; }  // Font index per line (per-screen: MapSize.Width * MapSize.Height * ScreenSize.Height)
            public Dictionary<string, ScreenReference> FontLineMappingReferences { get; set; }  // Key: "x,y" -> Value: referenced screen coordinates
            public bool? FontTemplateLocked { get; set; }  // Lock/unlock font templates
            public string FontTemplatePattern { get; set; }  // Template pattern name
            public bool? MultiFontEnabled { get; set; }  // Enable/disable multifont features
            public string MapDescription { get; set; }
            public Dictionary<string, string> ScreenDescriptions { get; set; }  // Key: "x,y"
            public Dictionary<string, ScreenMetadata> ScreenMetadataDict { get; set; }
            public string SubmapPath { get; set; }  // Path to submap file
            public bool? IsTilemap { get; set; }
            public TilemapData TilemapInfo { get; set; }
            public BitmapTilesetData BitmapTileset { get; set; }
            public Dictionary<string, LibraryElement> ElementLibrary { get; set; }
            public List<ScreenLink> ScreenLinks { get; set; }
        }
        public static Atrmap ParsedData { get; private set; }
        //public static JsonNode Node { get; set; }
       // public static byte[] ScreenMap { get; set; }
        public static void ParseAtrmap(string fileName)
        {
            // First, try to detect if it's a binary file by checking if it starts with binary formatter signature
            // BinaryFormatter files typically start with specific bytes
            bool isBinaryFile = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[4];
                    int bytesRead = fs.Read(header, 0, 4);
                    if (bytesRead == 4)
                    {
                        // BinaryFormatter serialized files often start with 0x00 0x01 0x00 0x00 or similar patterns
                        // Check if it looks like binary (not text/JSON)
                        isBinaryFile = !IsTextFile(header);
                    }
                }
            }
            catch
            {
                // If we can't read the file, fall through to normal processing
            }

            if (!isBinaryFile)
            {
                // Try to read as JSON first
                string json = File.ReadAllText(fileName);
                try
                {
                    ParsedData = JsonSerializer.Deserialize<Atrmap>(json, JsonOptions);
                    MigrateToV2IfNeeded();
                    return; // Successfully parsed as JSON
                }
                catch (JsonException)
                {
                    // Not valid JSON, will try binary format below
                }
            }

            // If JSON parsing failed or file appears to be binary, try to execute the AtariMapConverter
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "AtariMapConverter.exe";
                p.StartInfo.Arguments = $"\"{fileName}\"";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                
                string json = p.StandardOutput.ReadToEnd();
                string errors = p.StandardError.ReadToEnd();
                p.WaitForExit();
                
                if (p.ExitCode != 0 || string.IsNullOrWhiteSpace(json))
                {
                    throw new Exception($"AtariMapConverter failed with exit code {p.ExitCode}. Error: {errors}");
                }
                
                p.Dispose();
                
                try
                {
                    ParsedData = JsonSerializer.Deserialize<Atrmap>(json, JsonOptions);
                    MigrateToV2IfNeeded();
                }
                catch (JsonException ex2)
                {
                    throw new Exception($"Failed to parse converted JSON: {ex2.Message}. Converter output: {json}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading binary atrmap file: {ex.Message}");
                throw;
            }
        }

        private static bool IsTextFile(byte[] header)
        {
            // Check if the header looks like text (JSON typically starts with '{' or '[' or whitespace)
            // BinaryFormatter files don't start with these characters
            if (header.Length < 1) return true;
            
            byte firstByte = header[0];
            // JSON files typically start with: {, [, whitespace, or BOM
            // BinaryFormatter files start with serialization markers (usually 0x00 0x01 0x00 0x00 or similar)
            if (firstByte == 0x7B || firstByte == 0x5B || firstByte == 0x20 || firstByte == 0x09 || firstByte == 0x0A || firstByte == 0x0D)
                return true; // Looks like text (starts with {, [, space, tab, newline, or carriage return)
            
            // Check for UTF-8 BOM
            if (header.Length >= 3 && header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF)
                return true;
            
            // If first byte is 0x00, it's likely binary
            if (firstByte == 0x00)
                return false;
            
            // Default: assume it might be text if it's a printable ASCII character
            return firstByte >= 0x20 && firstByte < 0x7F;
        }

        private static void MigrateToV2IfNeeded()
        {
            if (ParsedData == null) return;

            // Check if this is an old version file
            bool isOldVersion = string.IsNullOrEmpty(ParsedData.AtrmapVersion) || 
                               ParsedData.AtrmapVersion == "1.2" ||
                               (ParsedData.FontDataArray == null && ParsedData.FontData != null);

            if (isOldVersion)
            {
                // Migrate FontData to FontDataArray
                if (ParsedData.FontDataArray == null && ParsedData.FontData != null)
                {
                    ParsedData.FontDataArray = new int[][] { ParsedData.FontData };
                }

                // Initialize FontLineMappingReferences if null (for old files)
                if (ParsedData.FontLineMappingReferences == null)
                {
                    ParsedData.FontLineMappingReferences = new Dictionary<string, ScreenReference>();
                }

                // Initialize other collections if null
                if (ParsedData.ScreenDescriptions == null)
                    ParsedData.ScreenDescriptions = new Dictionary<string, string>();
                if (ParsedData.ScreenMetadataDict == null)
                    ParsedData.ScreenMetadataDict = new Dictionary<string, ScreenMetadata>();
                if (ParsedData.ElementLibrary == null)
                    ParsedData.ElementLibrary = new Dictionary<string, LibraryElement>();
                if (ParsedData.ScreenLinks == null)
                    ParsedData.ScreenLinks = new List<ScreenLink>();

                // Set defaults
                if (!ParsedData.FontTemplateLocked.HasValue)
                    ParsedData.FontTemplateLocked = false;
                if (string.IsNullOrEmpty(ParsedData.FontTemplatePattern))
                    ParsedData.FontTemplatePattern = "All Font0";
                if (string.IsNullOrEmpty(ParsedData.MapDescription))
                    ParsedData.MapDescription = "";
            }
        }

        public static void SaveAtrMap(Atrmap atrmap, string fileName)
        {
            // Determine version based on whether new features are used
            bool hasV2Features = atrmap.FontDataArray != null || 
                                 atrmap.FontLineMappingPerScreen != null ||
                                 (atrmap.FontLineMappingReferences != null && atrmap.FontLineMappingReferences.Count > 0) ||
                                 !string.IsNullOrEmpty(atrmap.MapDescription) ||
                                 (atrmap.ScreenDescriptions != null && atrmap.ScreenDescriptions.Count > 0) ||
                                 (atrmap.ScreenMetadataDict != null && atrmap.ScreenMetadataDict.Count > 0) ||
                                 !string.IsNullOrEmpty(atrmap.SubmapPath) ||
                                 (atrmap.IsTilemap.HasValue && atrmap.IsTilemap.Value) ||
                                 atrmap.TilemapInfo != null ||
                                 atrmap.BitmapTileset != null ||
                                 (atrmap.ElementLibrary != null && atrmap.ElementLibrary.Count > 0) ||
                                 (atrmap.ScreenLinks != null && atrmap.ScreenLinks.Count > 0);

            if (hasV2Features)
            {
                atrmap.AtrmapVersion = "2.0";
            }
            else
            {
                atrmap.AtrmapVersion = "1.2";
            }

            // Always write FontData for backwards compatibility (from FontDataArray[0] if available)
            if (atrmap.FontData == null && atrmap.FontDataArray != null && atrmap.FontDataArray.Length > 0 && atrmap.FontDataArray[0] != null)
            {
                atrmap.FontData = atrmap.FontDataArray[0];
            }

            string json = JsonSerializer.Serialize(atrmap, JsonOptions);
            File.WriteAllText(fileName, json);
        }
    }
}
