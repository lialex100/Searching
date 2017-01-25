﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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

            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView1_DrawItem);
            this.listView1.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listView1_DrawSubItem);
            this.listView1.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listView1_RetrieveVirtualItem);
            this.listView1.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(this.listView1_DrawColumnHeader);
            //this.dataGridView1.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.dataGridView1_CellValueNeeded);

            _winSearch = new WinSearch("", 20);
            _results = _winSearch.Output2;
            _displayResults = _results;

            dataGridView1.VirtualMode = true;

            dataGridView1.RowCount = _results.Count;

            listView1.Columns.Add("int", 100);
            listView1.Columns.Add("Path", 100);
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.OwnerDraw = true;

            listView1.VirtualMode = true;

            foreach (var recodResult in _results)
            {
                // var item = new ListViewItem();
                //     var item = new ListViewItem(new string[] { recodResult.Number.ToString(), recodResult.Path });
                var item = new ListViewItem(new string[] { recodResult.Number.ToString(), recodResult.Path });
                //     item.Tag = recodResult;
                //   item.SubItems[0].Name = "int";
                //   item.SubItems[0].Text = recodResult.Number.ToString();
                //    item.SubItems.Add(recodResult.Path);

                //  listView1.Items.Add(item);
                dictionary.Add(recodResult.Number, item);
                displayItem.Add(recodResult);
            }

            listView1.VirtualListSize = _results.Count;
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {

            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }

                // Draw the standard header background.
                e.DrawBackground();
                // Draw the header text.
                using (Font headerFont =new Font("Microsoft Sans Serif", 8.25f))
                {
                    var r = e.Bounds;
                 //   r.
                //    e.Graphics.DrawString(e.Header.Text, listView1.Font, Brushes.Black, r, sf);
                    Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 5);
                    e.Graphics.DrawRectangle(blackPen, 10, 10, 100, 50);
                }
            }
            return;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            ListView listView = (ListView)sender;

            // Check if e.Item is selected and the ListView has a focus.
            if (!listView.Focused && e.Item.Selected)
            {
                Rectangle rowBounds = e.Bounds;
                int leftMargin = e.Item.GetBounds(ItemBoundsPortion.Label).Left;
                Rectangle bounds = new Rectangle(leftMargin, rowBounds.Top, rowBounds.Width - leftMargin, rowBounds.Height);
                e.Graphics.FillRectangle(SystemBrushes.Highlight, bounds);
            }
            else
                e.DrawDefault = true;

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

        //private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        //{
        //    return;

        //    if (e.RowIndex >= _displayResults.Count)
        //    {
        //        return;
        //    }
        //    // Set the cell value to paint using the Customer object retrieved.
        //    switch (this.dataGridView1.Columns[e.ColumnIndex].Name)
        //    {
        //        case "Path":
        //            e.Value = _displayResults[e.RowIndex].Path;
        //            break;
        //        case "FileName":
        //            e.Value = _displayResults[e.RowIndex].Number;
        //            break;
        //        case "Number":
        //            e.Value = _displayResults[e.RowIndex].Number;
        //            break;
        //    }
        //}

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            //    textFilename.Text = DateTime.UtcNow.ToString();

            if (dictionary != null && e.ItemIndex < displayItem.Count) //&& e.ItemIndex >= firstItem && e.ItemIndex < firstItem + myCache.Length)
            {
                //A cache hit, so get the ListViewItem from the cache instead of making a new one.
                //dictionary[e.ItemIndex];

                e.Item = dictionary[displayItem[e.ItemIndex].Number];
            }
            else
            {
                e.Item = new ListViewItem();
            }

        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            const int TEXT_OFFSET = 1;    // I don't know why the text is located at 1px to the right. Maybe it's only for me.

            ListView listView = (ListView)sender;

            // Check if e.Item is selected and the ListView has a focus.
            if (!listView.Focused && e.Item.Selected)
            {
                Rectangle rowBounds = e.SubItem.Bounds;
                Rectangle labelBounds = e.Item.GetBounds(ItemBoundsPortion.Label);
                int leftMargin = labelBounds.Left - TEXT_OFFSET;
                Rectangle bounds = new Rectangle(rowBounds.Left + leftMargin, rowBounds.Top, e.ColumnIndex == 0 ? labelBounds.Width : (rowBounds.Width - leftMargin - TEXT_OFFSET), rowBounds.Height);
                TextFormatFlags align;
                switch (listView.Columns[e.ColumnIndex].TextAlign)
                {
                    case HorizontalAlignment.Right:
                        align = TextFormatFlags.Right;
                        break;
                    case HorizontalAlignment.Center:
                        align = TextFormatFlags.HorizontalCenter;
                        break;
                    default:
                        align = TextFormatFlags.Left;
                        break;
                }
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, listView.Font, bounds, SystemColors.HighlightText,
                    align | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);
            }
            else
                e.DrawDefault = true;
        }
    }
}