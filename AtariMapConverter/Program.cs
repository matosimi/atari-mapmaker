using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtariMapConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("AtariMapConverter 1.0 (7.9.2022) for legacy AtariMapMaker (atrmap) binary format (version 1.1 or lower).\n");
                Console.WriteLine("Usage: AtariMapConverter myMap.atrmap");
                Console.WriteLine("Usage: AtariMapConverter myMap.atrmap > newMap.atrmap");

                return;
            }
            try
            {
                OldAtrmapDataProvider oadp = new OldAtrmapDataProvider();
                oadp.OpenAtrmap(args[0]);
                Console.Write(oadp.FlushData());
            }
            catch (Exception ex)
            {
                // Write error to stderr so it doesn't interfere with JSON output
                Console.Error.WriteLine($"Error converting file: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}
