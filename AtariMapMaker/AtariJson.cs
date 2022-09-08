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
            string json = File.ReadAllText(fileName);
            try
            {
                ParsedData = JsonSerializer.Deserialize<Atrmap>(json);
                //Node = JsonNode.Parse(json);
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
                //try to execute the AtariMapConverter
                Process p = new Process();
                p.StartInfo.FileName = "AtariMapConverter.exe";
                p.StartInfo.Arguments = $"\"{fileName}\"";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                json = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Dispose();
                try
                {
                    ParsedData = JsonSerializer.Deserialize<Atrmap>(json);
                    //Node = JsonNode.Parse(json);
                }
                catch (JsonException ex2)
                {
                    Console.WriteLine(ex2.Message);
                    return;
                }
            }
        }

        public static void SaveAtrMap(Atrmap atrmap, string fileName)
        {
            atrmap.AtrmapVersion = "1.2";
            string json = JsonSerializer.Serialize(atrmap);
            File.WriteAllText(fileName, json);
        }
    }
}
