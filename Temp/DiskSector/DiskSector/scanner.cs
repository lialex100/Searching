using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace DiskSector
{
    public static class MiscExtensions
    {
        // Ex: collection.TakeLast(5);
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }
    }

    public class scanner
    {
        public static void Main()
        {
            var sw = Stopwatch.StartNew();
            var files = (new DriveInfo(@"c:\")).EnumerateFiles().ToList();
            var elapsed = sw.ElapsedMilliseconds.ToString();
            Console.WriteLine(string.Format("Found {0} files, elapsed {1} ms", files.Count(), elapsed));
        }
    }

    /// <summary>
    /// https://dotblogs.com.tw/larrynung/2012/10/26/79041
    /// </summary>
    public class MFTScanner
    {
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private const uint GENERIC_READ = 0x80000000;
        private const int FILE_SHARE_READ = 0x1;
        private const int FILE_SHARE_WRITE = 0x2;
        private const int OPEN_EXISTING = 3;
        private const int FILE_READ_ATTRIBUTES = 0x80;
        private const int FILE_NAME_IINFORMATION = 9;
        private const int FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
        private const int FILE_OPEN_FOR_BACKUP_INTENT = 0x4000;
        private const int FILE_OPEN_BY_FILE_ID = 0x2000;
        private const int FILE_OPEN = 0x1;
        private const int OBJ_CASE_INSENSITIVE = 0x40;
        private const int FSCTL_ENUM_USN_DATA = 0x900b3;

        [StructLayout(LayoutKind.Sequential)]
        public struct MFT_ENUM_DATA
        {
            public long StartFileReferenceNumber;
            public long LowUsn;
            public long HighUsn;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct USN_RECORD
        {
            public int RecordLength;
            public short MajorVersion;
            public short MinorVersion;
            public long FileReferenceNumber;
            public long ParentFileReferenceNumber;
            public long Usn;
            public long TimeStamp;
            public int Reason;
            public int SourceInfo;
            public int SecurityId;
            public FileAttributes FileAttributes;
            public short FileNameLength;
            public short FileNameOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_STATUS_BLOCK
        {
            public int Status;
            public int Information;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public short Length;
            public short MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public int Attributes;
            public int SecurityDescriptor;
            public int SecurityQualityOfService;
        }

        //// MFT_ENUM_DATA
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(IntPtr hDevice, int dwIoControlCode, ref MFT_ENUM_DATA lpInBuffer,
            int nInBufferSize, IntPtr lpOutBuffer,
            int nOutBufferSize, ref int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice, WINBASEConstants dwIoControlCode,
            [In] ref MFT_ENUM_DATA lpInBuffer, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize, ref uint lpBytesReturned, [In] ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int CloseHandle(IntPtr lpObject);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int NtCreateFile(ref IntPtr FileHandle, int DesiredAccess, ref OBJECT_ATTRIBUTES ObjectAttributes, ref IO_STATUS_BLOCK IoStatusBlock, int AllocationSize, int FileAttribs, int SharedAccess, int CreationDisposition,
            int CreateOptions, int EaBuffer,
            int EaLength);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int NtQueryInformationFile(IntPtr FileHandle, ref IO_STATUS_BLOCK IoStatusBlock, IntPtr FileInformation, int Length, int FileInformationClass);

        private IntPtr m_hCJ;
        private IntPtr m_Buffer;
        private int m_BufferSize;

        private string m_DriveLetter;

        private class FSNode
        {
            public long FRN;
            public long ParentFRN;
            public string FileName;

            public FileAttributes FileAttributes;

            public FSNode(long lFRN, long lParentFSN, string sFileName, FileAttributes fileAttributes)
            {
                FRN = lFRN;
                ParentFRN = lParentFSN;
                FileName = sFileName;
                FileAttributes = fileAttributes;
            }

            public override string ToString()
            {
                return $"FileName : {FileName}  FRN : {FRN}";
            }
        }

        private IntPtr OpenVolume(string szDriveLetter)
        {
            IntPtr hCJ = default(IntPtr);
            //// volume handle

            m_DriveLetter = szDriveLetter;

            hCJ = CreateFile("\\\\.\\" + szDriveLetter, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            return hCJ;
        }

        private SafeFileHandle OpenVolume2(string szDriveLetter)
        {
            m_DriveLetter = szDriveLetter;
            var hCJ = Class1.CreateFile("\\\\.\\" + szDriveLetter, EFileAccess.GenericRead, EFileShare.Read | EFileShare.Write, IntPtr.Zero, ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            return hCJ;
        }

        private void Cleanup()
        {
            if (m_hCJ != IntPtr.Zero)
            {
                // Close the volume handle.
                CloseHandle(m_hCJ);
                m_hCJ = INVALID_HANDLE_VALUE;
            }

            if (m_Buffer != IntPtr.Zero)
            {
                // Free the allocated memory
                Marshal.FreeHGlobal(m_Buffer);
                m_Buffer = IntPtr.Zero;
            }
        }

        public IEnumerable<string> EnumerateFiles(string szDriveLetter)
        {
            try
            {
                var usnRecord = default(USN_RECORD);
                var mft = default(MFT_ENUM_DATA);
                var dwRetBytes = 0;
                var dwBytes = 0;
                var cb = 0;
                var dicFRNLookup = new Dictionary<long, FSNode>(1900000);
                var bIsFile = false;

                // This shouldn't be called more than once.
                if (m_Buffer.ToInt32() != 0)
                {
                    throw new Exception("invalid buffer");
                }

                // Assign buffer size
                m_BufferSize = (1024 * 1024);

                // Allocate a buffer to use for reading records.
                m_Buffer = Marshal.AllocHGlobal(m_BufferSize);

                // correct path
                szDriveLetter = szDriveLetter.TrimEnd('\\');

                // Open the volume handle 
                m_hCJ = OpenVolume(szDriveLetter);

                // Check if the volume handle is valid.
                if (m_hCJ == INVALID_HANDLE_VALUE)
                {
                    throw new Exception("Couldn't open handle to the volume.");
                }
                var hcj = OpenVolume2(szDriveLetter);

                if (hcj.IsInvalid)
                {
                    throw new Exception("Couldn't open handle to the volume.");
                }

                //mft.StartFileReferenceNumber = 0;
                //mft.LowUsn = 0;
                //mft.HighUsn = long.MaxValue;

                MFT_ENUM_DATA mftobj = new MFT_ENUM_DATA();
                mftobj.StartFileReferenceNumber = 0;
                mftobj.LowUsn = 0;
                mftobj.HighUsn = long.MaxValue;
                do
                {
                    //var mftsize = Convert.ToUInt32(Marshal.SizeOf(mftobj));

                    //NativeOverlapped overlapped = default(NativeOverlapped);
                    //uint bytereturn = 0;
                    //var tep = DeviceIoControl(hcj, WINBASEConstants.FSCTL_ENUM_USN_DATA, ref mftobj, mftsize,
                    //    m_Buffer, Convert.ToUInt32(m_BufferSize), ref bytereturn, ref overlapped);

                    if (DeviceIoControl(m_hCJ, FSCTL_ENUM_USN_DATA, ref mftobj, Marshal.SizeOf(mftobj),
                        m_Buffer, m_BufferSize, ref dwBytes, IntPtr.Zero))
                    {
                        dwRetBytes = dwBytes - sizeof(long);

                        var dd = sizeof(long);
                        // Pointer to the first record
                        IntPtr pUsnRecord = new IntPtr(m_Buffer.ToInt32() + sizeof(long));
                        usnRecord = (USN_RECORD) Marshal.PtrToStructure(pUsnRecord, typeof(USN_RECORD));

                        while ((dwRetBytes > 0))
                        {
                            // The filename within the USN_RECORD.
                            string FileName = Marshal.PtrToStringUni(new IntPtr(pUsnRecord.ToInt32() + usnRecord.FileNameOffset), usnRecord.FileNameLength / 2);

                            // bIsFile = !usnRecord.FileAttributes.HasFlag(FileAttributes.Directory);
                            dicFRNLookup.Add(usnRecord.FileReferenceNumber, new FSNode(usnRecord.FileReferenceNumber, usnRecord.ParentFileReferenceNumber, FileName, usnRecord.FileAttributes));

                            dwRetBytes -= usnRecord.RecordLength;
                            // Pointer to the next record in the buffer.
                            pUsnRecord = new IntPtr(pUsnRecord.ToInt32() + usnRecord.RecordLength);
                            // Copy pointer to USN_RECORD structure.
                            usnRecord = (USN_RECORD) Marshal.PtrToStructure(pUsnRecord, typeof(USN_RECORD));
                        }

                        // The first 8 bytes is always the start of the next USN.
                        mftobj.StartFileReferenceNumber = Marshal.ReadInt64(m_Buffer, 0);
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                var last10 = dicFRNLookup.ToList().TakeLast(10).ToList();

                // Resolve all paths for Files
                foreach (FSNode oFSNode in dicFRNLookup.Values.Where(o => (o.FileAttributes & FileAttributes.Directory) != 0))
                {
                    string sFullPath = oFSNode.FileName;
                    FSNode oParentFSNode = oFSNode;

                    while (dicFRNLookup.TryGetValue(oParentFSNode.ParentFRN, out oParentFSNode))
                    {
                        sFullPath = string.Concat(oParentFSNode.FileName, "\\", sFullPath);
                    }
                    sFullPath = string.Concat(szDriveLetter, "\\", sFullPath);

                    yield return sFullPath;
                }
            }
            finally
            {
                //// cleanup
                Cleanup();
            }
        }
    }

    public static class DriveInfoExtension
    {
        public static IEnumerable<string> EnumerateFiles(this DriveInfo drive)
        {
            return (new MFTScanner()).EnumerateFiles(drive.Name);
        }
    }
}