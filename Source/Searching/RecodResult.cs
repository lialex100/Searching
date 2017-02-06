using System.IO;
using Newtonsoft.Json;

namespace Searching
{
    public class RecodResult
    {
        //pimary key  or FileReferenceNumber
        public long Number { get; set; } 

        [JsonIgnore]
        public string Raw { get; set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Path { get; set; }

        public long TimeStamp { get; set; }
        public long FileReferenceNumber { get; set; }
        public long ParentFileReferenceNumber { get; set; }
        public FileAttributes FileAttributes { get; set; }
    }
}