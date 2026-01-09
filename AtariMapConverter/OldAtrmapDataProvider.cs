using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AtariMapConverter
{
    [Serializable]
    public class AtariMap
    {
        public byte[] data;
        public int[] Data { get { return data?.Select(b => (int)b).ToArray() ?? new int[0]; } }
        public Size screenSize;    
        public Size screens;       
    }

    [Serializable]
    public class AtariFontRenderer
    {
        public byte[] fontData;
        public int[] FontData { get { return fontData?.Select(b => (int)b).ToArray() ?? new int[0]; } }
        public byte[] color5;
        public int[] Color5 { get { return color5?.Select(b => (int)b).ToArray() ?? new int[0]; } }
    }

    // Proxy for unknown AtariMapMaker types we need to skip
    [Serializable]
    public class SkipProxy { }

    // Binder to map AtariMapMaker types to AtariMapConverter types
    public class AtariMapBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            // Map known AtariMapMaker types to AtariMapConverter types
            if (typeName == "AtariMapMaker.AtariMap" && assemblyName.Contains("AtariMapMaker"))
                return typeof(AtariMap);
            if (typeName == "AtariMapMaker.AtariFontRenderer" && assemblyName.Contains("AtariMapMaker"))
                return typeof(AtariFontRenderer);
            
            // Skip any other AtariMapMaker types (like AtariPalette)
            if (assemblyName.Contains("AtariMapMaker"))
                return typeof(SkipProxy);
            
            return null; // Use default for other types
        }
    }

    internal class OldAtrmapDataProvider
    {
        private AtariMap myMap;
        private AtariFontRenderer myRenderer;

        public void OpenAtrmap(string fileName)
        {
            // Handle missing AtariMapMaker assembly
            ResolveEventHandler handler = (s, e) =>
                e.Name.Contains("AtariMapMaker") ? Assembly.GetExecutingAssembly() : null;
            AppDomain.CurrentDomain.AssemblyResolve += handler;
            
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = new AtariMapBinder();
                System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open);
                try
                {
                    // Deserialize first object (should be AtariMap)
                    object obj = bf.Deserialize(fs);
                    if (obj is AtariMap map)
                        myMap = map;
                    else
                        throw new InvalidOperationException($"Expected AtariMap, got {obj?.GetType().FullName ?? "null"}");
                    
                    // Skip any SkipProxy objects, find AtariFontRenderer
                    object rendererObj;
                    do
                    {
                        rendererObj = bf.Deserialize(fs);
                    } while (rendererObj is SkipProxy && fs.Position < fs.Length);
                    
                    if (rendererObj is AtariFontRenderer renderer)
                        myRenderer = renderer;
                    else
                        throw new InvalidOperationException($"Expected AtariFontRenderer, got {rendererObj?.GetType().FullName ?? "null"}");
                }
                finally
                {
                    fs.Close();
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= handler;
            }
        }
        public  byte[] GetFontData()
        {
            return myRenderer.fontData;
        }

        public  byte[] GetColor5()
        {
            return myRenderer.color5;
        }

        public  byte[] GetMapData()
        {
            return myMap.data;
        }

        public  Size GetMapSize()
        {
            return myMap.screens;
        }

        public  Size GetScreenSize()
        {
            return myMap.screenSize;
        }

        public  string FlushData()
        {

            JsonObject jo = new JsonObject
            {
                { "AtrmapVersion", "1.1"},
                { "MapData", JsonValue.Create(myMap.Data) },
                { "MapSize", JsonValue.Create(myMap.screens) },
                { "MapScreenSize", JsonValue.Create(myMap.screenSize) },
                { "FontData", JsonValue.Create(myRenderer.FontData) },
                { "Color5", JsonValue.Create(myRenderer.Color5) }
            };

            string jsonString = JsonSerializer.Serialize(jo);
            return jsonString;
        }
    }
}
