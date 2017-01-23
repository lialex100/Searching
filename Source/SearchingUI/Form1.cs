using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private BindingSource _bindingSource;

        public Form1()
        {
            InitializeComponent();
            _winSearch = new WinSearch("", 2000);
            _results = _winSearch.Output2;

            dataGridView1.AutoGenerateColumns = true;

            _bindingSource = new BindingSource(_results, null);
            dataGridView1.DataSource = _bindingSource;
        }

        private void textPath_TextChanged(object sender, EventArgs e)
        {
            //   var filted = _results.Where(x => x.Path.Contains(textPath.Text)).ToList();

            //   var bindingList = new BindingList<RecodResult>(filted);

            //  var source = new BindingSource(_results, null);

            //     int i = Int32.Parse(textPath.Text);
            var num = _results.Where(x => x.Path.Contains(textPath.Text, StringComparison.OrdinalIgnoreCase)).Select(x => x.Number).ToList();
            if (!num.Any())
            {
                return;
            }
            _bindingSource.SuspendBinding();

            var filterIterator = 0;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if ( filterIterator < num.Count && num[filterIterator] == i )
                {
                    dataGridView1.Rows[i].Visible = true;
                    filterIterator++;
                }
                else
                {
                    dataGridView1.Rows[i].Visible = false;
                }
            }
            _bindingSource.ResumeBinding();

   //         dataGridView1.Refresh();
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
            dataGridView1.DataSource = source;
        }
    }
}