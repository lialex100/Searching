using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
/// <summary>
/// http://stackoverflow.com/questions/724148/is-there-a-faster-way-to-scan-through-a-directory-recursively-in-net/724184#724184
/// </summary>
namespace DiskSector
{
    public static class FILETIMEExtensions
    {
        public static DateTime ToDateTime(this FILETIME filetime)
        {
            long highBits = filetime.dwHighDateTime;
            highBits = highBits << 32;
            return DateTime.FromFileTimeUtc(highBits + (long)filetime.dwLowDateTime);
        }
    }

    public class Class1
    {
        public static void MainX()
        {
            var info = RecursiveScan2("c:\\");


        }

        static List<Info> RecursiveScan2(string directory)
        {
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            WIN32_FIND_DATAW findData;
            IntPtr findHandle = INVALID_HANDLE_VALUE;

            var info = new List<Info>();
            try
            {
                findHandle = FindFirstFileW(directory + @"\*", out findData);
                if (findHandle != INVALID_HANDLE_VALUE)
                {
                    do
                    {
                        if (findData.cFileName == "." || findData.cFileName == "..") continue;

                        string fullpath = directory + (directory.EndsWith("\\") ? "" : "\\") + findData.cFileName;

                        bool isDir = false;

                        if ((findData.dwFileAttributes & FileAttributes.Directory) != 0)
                        {
                            isDir = true;
                            info.AddRange(RecursiveScan2(fullpath));
                        }

                        info.Add(new Info()
                        {
                            CreatedDate = findData.ftCreationTime.ToDateTime(),
                            ModifiedDate = findData.ftLastWriteTime.ToDateTime(),
                            IsDirectory = isDir,
                            Path = fullpath
                        });
                    } while (FindNextFile(findHandle, out findData));
                }
            }
            finally
            {
                if (findHandle != INVALID_HANDLE_VALUE) FindClose(findHandle);
            }
            return info;
        }

        class Info
        {
            public bool IsDirectory;
            public string Path;
            public DateTime ModifiedDate;
            public DateTime CreatedDate;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
               string lpFileName,
               EFileAccess dwDesiredAccess,
               EFileShare dwShareMode,
               IntPtr lpSecurityAttributes,
               ECreationDisposition dwCreationDisposition,
               EFileAttributes dwFlagsAndAttributes,
               IntPtr hTemplateFile);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATAW
        {
            public FileAttributes dwFileAttributes;
            internal FILETIME ftCreationTime;
            internal FILETIME ftLastAccessTime;
            internal FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public string cAlternateFileName;
        }
    }
}