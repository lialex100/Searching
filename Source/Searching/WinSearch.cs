using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Searching
{

    //https://msdn.microsoft.com/en-us/library/bb231256(v=vs.85).aspx
    //https://github.com/LordMike/NtfsLib/tree/master/NTFSLib.Helpers
    //http://stackoverflow.com/questions/31938722/how-to-read-file-attributes-using-master-file-table-data

    public class WinSearch
    {
        public IList<string> Output { get; set; }
        public IList<RecodResult> Output2 { get; set; }

        private readonly string connectionString = "Provider=Search.CollatorDSO;Extended Properties='Application=Windows';";

        public WinSearch(string searchString = null)
        {
            ExecuteSearch(searchString);
        }



        public void ExecuteSearch(string searchString = null)
        {

            OleDbConnection conn = new OleDbConnection(connectionString);
            OleDbCommand cmd = new OleDbCommand($"SELECT System.ItemPathDisplay, System.ItemType FROM SYSTEMINDEX "
                                                //+ $"Where System.ItemType='{ItemType.Directory:G}'"
                                                , conn);

            OleDbDataReader rdr = null;

            conn.Open();

            rdr = cmd.ExecuteReader();

            var output = new List<string>();
            while (rdr.Read())
            {
                string x = string.Empty;
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    x += rdr.GetValue(i) + " | ";
                }
                output.Add(x);
            }

            rdr.Close();
            conn.Close();
            Output = output;
        }
    }

    public enum ItemType
    {
        
        Folder,
        Directory,


    }


}