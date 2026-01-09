using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace AtariMapMaker
{
    internal static class AtariJson
    {
        public class Atrmap
        {
            public string AtrmapVersion { get; set; }
            public int[] MapData { get; set; }
            public int[] FontData { get; set; }
            public int[] Color5 { get; set; }
            public Size MapSize { get; set; }
            public Size MapScreenSize { get; set; }
            public int[] DliData { get; set; }
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
                    ParsedData = JsonSerializer.Deserialize<Atrmap>(json);
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
                    ParsedData = JsonSerializer.Deserialize<Atrmap>(json);
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

        public static void SaveAtrMap(Atrmap atrmap, string fileName)
        {
            atrmap.AtrmapVersion = "1.2";
            string json = JsonSerializer.Serialize(atrmap);
            File.WriteAllText(fileName, json);
        }
    }
}
