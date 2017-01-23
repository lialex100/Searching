using System;
using System.IO;
using Newtonsoft.Json;

namespace Searching
{
    public class RecodResult
    {
        public RecodResult()
        {
            Visible = true;
        }

        [JsonIgnore]
        public String Raw { get; set; }
        public String Path { get; set; }  
        public String FileType { get; set; }
        public int Number { get; set; }
        public bool Visible { get; set; }
    }
}