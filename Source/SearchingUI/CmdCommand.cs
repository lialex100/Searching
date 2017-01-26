using System.Diagnostics;
using System.IO;

namespace SearchingUI
{
    public class CmdCommand
    {
        public static void OpenFolder(string fileName)
        {
            if (File.Exists(fileName) || Directory.Exists(fileName))
            {
                var fol = new ProcessStartInfo("Explorer.exe", "/select," + fileName);
                Process.Start(fol);
            }
        }

        public static void OpenVisualStudio(string fileName)
        {
            if (File.Exists(fileName) || Directory.Exists(fileName))
            {
                var fol = new ProcessStartInfo(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe", $"/edit \"{fileName}\"");
                Process.Start(fol);
            }
        }

        public static void SystemDefaultOpen(string fileName)
        {
            if (File.Exists(fileName) || Directory.Exists(fileName))
            {
                Process.Start(fileName);
            }
        }
    }
}