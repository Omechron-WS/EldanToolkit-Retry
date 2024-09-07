using LibNexus.Files;
using System.Diagnostics;
using System.IO;

namespace EldanToolkit.Libraries.LibNexus.Files
{
    public static class MyExtensions
    {
        public static void Goto<T>(this Stream stream, string field, ulong position, bool debug)
        {
            FileFormatException.ThrowIf<T>(field, stream.Position != (long)position && debug);
            stream.Position = (long)position;
        }
    }
}
