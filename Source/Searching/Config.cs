using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Searching
{
    public class Config
    {
        public List<RecodResult> Records { get; set; }

        public bool Save()
        {
            Records.Sort((emp1, emp2) => String.Compare(emp1.Path, emp2.Path, StringComparison.Ordinal));

          //  var Rs = Records.Where(x => x.FileType.StartsWith("."));

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(@"./indexFile.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, Records);
            }

            return true;
        }

        public bool Load()
        {
            return true;
        }

    }
}