using System;
using System.IO;
using Newtonsoft.Json;

namespace Searching
{
    public class RecodResult
    {
        [JsonIgnore]
        public String Raw { get; set; }
        public String Path { get; set; }  
        public String FileType { get; set; }
        public int Number { get; set; }
    }
}