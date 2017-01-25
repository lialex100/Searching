using System.IO;
using System.IO.Compression;

namespace Searching
{
    public class CompressStream
    {
        private static string directoryPath = @".\";
        public static void Compress(DirectoryInfo directorySelected)
        {

            //using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
            //{
            //    using (GZipStream compressionStream = new GZipStream(compressedFileStream,
            //       CompressionMode.Compress))
            //    {
            //        originalFileStream.CopyTo(compressionStream);

            //    }
            //}
            FileInfo info = new FileInfo($"{directoryPath}index.gz");
            //         Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
            //  fileToCompress.Name, fileToCompress.Length.ToString(), info.Length.ToString());

        }

    }
}