using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AtariMapMaker
{
    [Serializable]
    public class AtariMap
    {
        public byte[] data;
        public int[] Data { get { return data.Select(b => (int)b).ToArray(); } }
        public Size screenSize;    
        public Size screens;       
    }

    [Serializable]
    public class AtariFontRenderer
    {
        public byte[] fontData;
        public int[] FontData { get { return fontData.Select(b => (int)b).ToArray(); } }
        public byte[] color5;
        public int[] Color5 { get { return color5.Select(b => (int)b).ToArray(); } }
    }

    internal class OldAtrmapDataProvider
    {
        private AtariMap myMap;
        private AtariFontRenderer myRenderer;

        public void OpenAtrmap(string fileName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open); //test.dat
            myMap = (AtariMap)bf.Deserialize(fs);
            myRenderer = (AtariFontRenderer)bf.Deserialize(fs);
            fs.Close();
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
