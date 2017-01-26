using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Searching
{
    public class Config
    {
        public List<RecodResult> Records { get; set; }
        public string Filename { get; set; } = @".\index.gz";

        public bool SaveTofile()
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

        public bool Save()
        {

            Records.Sort((emp1, emp2) => String.Compare(emp1.Path, emp2.Path, StringComparison.Ordinal));
            var i = 0;
            Records.ForEach(x => x.Number = i++);

            JsonSerializer serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

            using (MemoryStream memory = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(memory))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                // store json in memory 
                serializer.Serialize(writer, Records);
                sw.Flush();

                // write to file
                using (FileStream f2 = new FileStream(Filename, FileMode.Create))
                using (GZipStream gz = new GZipStream(f2, CompressionMode.Compress, false))
                {
                    memory.WriteTo(gz);
                }
            }
            return true;
        }

        public bool Load()
        {
            using (FileStream originalFileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    StreamReader re = new StreamReader(decompressionStream);
                    using (JsonReader reader = new JsonTextReader(re))
                    {
                        var serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
                        var temp = serializer.Deserialize<List<RecodResult>>(reader);
                        temp.AsParallel().ForAll(x =>
                        {
                            x.Path = x.Path.Replace("/", "\\");
                        });
                        Records = temp;
                    }
                }
            }
            return true;
        }
    }
}