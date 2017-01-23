using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Searching;
using SearchingUI.Helper;

namespace SearchingUI
{
    public partial class Form1 : Form
    {
        private WinSearch _winSearch;
        private List<RecodResult> _results;
        private List<RecodResult> _displayResults;
        private BindingSource _bindingSource;
        private Dictionary<int, ListViewItem> dictionary = new Dictionary<int, ListViewItem>();
        private List<RecodResult> displayItem = new List<RecodResult>();

        public Form1()
        {
            InitializeComponent();
            _winSearch = new WinSearch("", 0);
            _results = _winSearch.Output2;
            _displayResults = _results;

            dataGridView1.VirtualMode = true;

            dataGridView1.RowCount = _results.Count;

            listView1.Columns.Add("int", 100);
            listView1.Columns.Add("Path", 100);
            listView1.View = View.Details;
            listView1.OwnerDraw = true;

            listView1.VirtualMode = true;

            foreach (var recodResult in _results)
            {
                var ss = recodResult.GetHashCode();

                var item = new ListViewItem(new string[] { recodResult.Number.ToString(), recodResult.Path });
                //  listView1.Items.Add(item);
                dictionary.Add(recodResult.Number, item);
                displayItem.Add(recodResult);
            }

            listView1.VirtualListSize = 100;
        }

        private async void textPath_TextChanged(object sender, EventArgs e)
        {
            //   var filted = _results.Where(x => x.Path.Contains(textPath.Text)).ToList();

            //   var bindingList = new BindingList<RecodResult>(filted);

            //  var source = new BindingSource(_results, null);

            //     int i = Int32.Parse(textPath.Text);
            //  dataGridView1.CurrentCell = null;
            // _bindingSource.SuspendBinding();

            Stopwatch sw = new Stopwatch();


            //   listView1.Items.Clear();
         
            this.BeginInvoke((Action)delegate ()
           {
               sw.Start();
               //   ChangeRow();
               //  AddRow();

               displayItem = FilterRecord(textPath.Text).ToList();
               listView1.VirtualListSize = displayItem.Count;
               sw.Stop();

               listView1.Refresh();
               label2.Text = sw.Elapsed.TotalSeconds.ToString();
           });

            //dataGridView1.RowCount = _displayResults.Count;



            // dataGridView1.Refresh();
        }

        IEnumerable<RecodResult> FilterRecord(string searchText)
        {
            return _winSearch.Output2.Where(x => x.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        private void ChangeRow()
        {
            string searchText = textPath.Text;
            _displayResults = _winSearch.Output2.Where(x => x.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
            for (int i = 0; i < _displayResults.Count; i++)
            {
                listView1.Items[i].SubItems[0].Text = _displayResults[i].Number.ToString();
                listView1.Items[i].SubItems[1].Text = _displayResults[i].Path;
            }
        }

        private void AddRow()
        {
            string searchText = textPath.Text;
            _displayResults = _winSearch.Output2.Where(x => x.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

            var list = new List<ListViewItem>(1000);
            foreach (var recodResult in _displayResults)
            {
                list.Add(new ListViewItem(new string[] { recodResult.Number.ToString(), recodResult.Path }));
            }
            listView1.Items.AddRange(list.ToArray());
        }

        private void HideRow(int from)
        {
            for (int i = from - 1; i < dataGridView1.RowCount; i++)
            {
                //  dataGridView1.Rows[i].Visible = false;
            }
        }

        private BindingSource GetValue()
        {
            var filtered = _results.Where(x => x.Path.Contains(textPath.Text, StringComparison.OrdinalIgnoreCase));

            return new BindingSource(filtered.ToArray(), null);
        }

        private void textFilename_TextChanged(object sender, EventArgs e)
        {
            var filted = _results.Where(x => x.Path.Contains(textPath.Text)).ToList();

            //   var bindingList = new BindingList<RecodResult>(filted);
            var source = new BindingSource(filted, null);
            //  dataGridView1.DataSource = source;
            dataGridView1.RowCount = 0;
        }

        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            return;

            if (e.RowIndex >= _displayResults.Count)
            {
                return;
            }
            // Set the cell value to paint using the Customer object retrieved.
            switch (this.dataGridView1.Columns[e.ColumnIndex].Name)
            {
                case "Path":
                    e.Value = _displayResults[e.RowIndex].Path;
                    break;
                case "FileName":
                    e.Value = _displayResults[e.RowIndex].Number;
                    break;
                case "Number":
                    e.Value = _displayResults[e.RowIndex].Number;
                    break;
            }
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
        //    textFilename.Text = DateTime.UtcNow.ToString();

            if (dictionary != null && e.ItemIndex < displayItem.Count) //&& e.ItemIndex >= firstItem && e.ItemIndex < firstItem + myCache.Length)
            {
                //A cache hit, so get the ListViewItem from the cache instead of making a new one.
                //dictionary[e.ItemIndex];

                e.Item = dictionary[displayItem[e.ItemIndex].Number];
            }
        }
    }
}