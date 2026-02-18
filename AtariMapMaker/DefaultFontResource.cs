using System;

namespace AtariMapMaker
{
    /// <summary>
    /// Returns default font data (1024 + 1024 inverted) from resources. Used when no font is loaded (e.g. new map, AssignWindow).
    /// </summary>
    public static class DefaultFontResource
    {
        /// <summary>Returns default font data (1024 + 1024 inverted). Never returns null.</summary>
        public static byte[] GetDefaultFontDataFromResources()
        {
            byte[] result = new byte[1024 * 2];
            try
            {
                byte[] res = Properties.Resources.Default;
                if (res != null && res.Length >= 1024)
                {
                    Array.Copy(res, result, 1024);
                    for (int a = 0; a < 1024; a++)
                        result[a + 1024] = (byte)(result[a] ^ 0x80);
                }
            }
            catch
            {
                // Keep zeros so renderer still has valid buffer
            }
            return result;
        }
    }
}
