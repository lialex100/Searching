using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Helper.Helper;
using Searching;

namespace SearchingUI
{
    public partial class Form1 : Form
    {
        //  private WinSearch _winSearch;
        private List<RecodResult> _results;
        private List<RecodResult> _displayResults;
        private Dictionary<int, ListViewItem> dictionary = new Dictionary<int, ListViewItem>();
        private List<RecodResult> displayItem = new List<RecodResult>();
        private ContextMenuStrip fruitContextMenuStrip;
        private ContextMenu _myContextMenu;

        void initRightClick()
        {
            // Create a new ContextMenuStrip control.
            fruitContextMenuStrip = new ContextMenuStrip();

            // Attach an event handler for the 
            // ContextMenuStrip control's Opening event.
            //     fruitContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(cms_Opening);

            // Create a new ToolStrip control.
            ToolStrip ts = new ToolStrip();

            // Create a ToolStripDropDownButton control and add it
            // to the ToolStrip control's Items collections.
            ToolStripDropDownButton fruitToolStripDropDownButton = new ToolStripDropDownButton("Fruit", null, null, "Fruit");
            ts.Items.Add(fruitToolStripDropDownButton);

            // Dock the ToolStrip control to the top of the form.
            ts.Dock = DockStyle.Top;

            // Assign the ContextMenuStrip control as the 
            // ToolStripDropDownButton control's DropDown menu.
            fruitToolStripDropDownButton.DropDown = fruitContextMenuStrip;

            // Create a new MenuStrip control and add a ToolStripMenuItem.
            MenuStrip ms = new MenuStrip();
            ToolStripMenuItem fruitToolStripMenuItem = new ToolStripMenuItem("Fruit", null, null, "Fruit");
            ms.Items.Add(fruitToolStripMenuItem);

            // Dock the MenuStrip control to the top of the form.
            ms.Dock = DockStyle.Top;

            // Assign the MenuStrip control as the 
            // ToolStripMenuItem's DropDown menu.
            fruitToolStripMenuItem.DropDown = fruitContextMenuStrip;

            // Assign the ContextMenuStrip to the form's 
            // ContextMenuStrip property.
            ContextMenuStrip = fruitContextMenuStrip;

            // Add the ToolStrip control to the Controls collection.
            Controls.Add(ts);

            //Add a button to the form and assign its ContextMenuStrip.
            Button b = new Button();
            b.Location = new Point(60, 60);
            Controls.Add(b);
            b.ContextMenuStrip = fruitContextMenuStrip;

            // Add the MenuStrip control last.
            // This is important for correct placement in the z-order.
            Controls.Add(ms);
        }

        public Form1()
        {
            InitializeComponent();
            InitContextmenu();

            listView1.DrawItem += new DrawListViewItemEventHandler(listView1_DrawItem);
            listView1.DrawSubItem += new DrawListViewSubItemEventHandler(listView1_DrawSubItem);
            listView1.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(listView1_RetrieveVirtualItem);
            listView1.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(listView1_DrawColumnHeader);

            //     listView1.Columns.Add("No");
            //        listView1.Columns.Add("Path");

            //this.dataGridView1.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.dataGridView1_CellValueNeeded);
            Config conf = new Config();

            if (File.Exists(conf.Filename))
            {
                conf.Load();
                _results = conf.Records;
            }
            else
            {
                var WinSearch = new WinSearch("", 6000);
                _results = WinSearch.Output2;
                Task.Run(() =>
                {
                    conf.Records = _results;
                    conf.Save();
                });
            }

            _displayResults = _results;

            //dataGridView1.VirtualMode = true;

            //  dataGridView1.RowCount = _results.Count;

            listView1.Columns.Add("", 100);
            listView1.Columns.Add("int", 100);
            listView1.Columns.Add("Path", 100);
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.OwnerDraw = true;

            listView1.VirtualMode = true;

            foreach (var recodResult in _results)
            {
                // var item = new ListViewItem();
                //    var item = new ListViewItem(new string[] { recodResult.Number.ToString(), recodResult.Path });

                ListViewItem item = new ListViewItem();
                //  item.im = -1;
                // item.SubItems.Add("");
                item.SubItems.Add(recodResult.Number.ToString());
                item.SubItems.Add(recodResult.Path);

                dictionary.Add(recodResult.Number, item);
                displayItem.Add(recodResult);
            }

            listView1.VirtualListSize = _results.Count;
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.Columns[0].Width = 70;

            //   listView1.Bounds = new Rectangle(new Point(10, 10), new Size(300, 200));
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            return;

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
                using (Font headerFont = new Font("Microsoft Sans Serif", 8.25f))
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
            ListView listView = (ListView) sender;

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

            //   listView1.Items.Clear();
            var temp = textPath.Text;
            BeginInvoke((Action) delegate()
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //   ChangeRow();
                //  AddRow();

                displayItem = FilterRecord(temp).ToList();
                listView1.VirtualListSize = displayItem.Count;
                sw.Stop();

                listView1.Refresh();

                var dd = sw.Elapsed.TotalSeconds.ToString();
                label2.Text = dd;
            });
        }

        IEnumerable<RecodResult> FilterRecord(string searchText)
        {
            return _results.AsParallel().AsOrdered().Where(x => x.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            //return _results.AsParallel().Where(x =>
            //{
            //    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(x.Path, searchText, 0, x.Path.Length, CompareOptions.IgnoreCase) > 0;
            //});
        }

        //private void ChangeRow()
        //{
        //    string searchText = textPath.Text;
        //    _displayResults = _results.Where(x => x.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        //    for (int i = 0; i < _displayResults.Count; i++)
        //    {
        //        listView1.Items[i].SubItems[0].Text = _displayResults[i].Number.ToString();
        //        listView1.Items[i].SubItems[1].Text = _displayResults[i].Path;
        //    }
        //}

        //private void AddRow()
        //{
        //    string searchText = textPath.Text;
        //    _displayResults = _results.Where(x => x.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

        //    var list = new List<ListViewItem>(1000);
        //    foreach (var recodResult in _displayResults)
        //    {
        //        list.Add(new ListViewItem(new string[] {recodResult.Number.ToString(), recodResult.Path}));
        //    }
        //    listView1.Items.AddRange(list.ToArray());
        //}

        //private void HideRow(int from)
        //{
        //    for (int i = from - 1; i < dataGridView1.RowCount; i++)
        //    {
        //        //  dataGridView1.Rows[i].Visible = false;
        //    }
        //}

        //private BindingSource GetValue()
        //{
        //    var filtered = _results.Where(x => x.Path.Contains(textPath.Text, StringComparison.OrdinalIgnoreCase));

        //    return new BindingSource(filtered.ToArray(), null);
        //}

        private void textFilename_TextChanged(object sender, EventArgs e)
        {
            var filted = _results.Where(x => x.Path.Contains(textPath.Text)).ToList();

            //   var bindingList = new BindingList<RecodResult>(filted);
            var source = new BindingSource(filted, null);
            //  dataGridView1.DataSource = source;
            //  dataGridView1.RowCount = 0;
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
            const int TEXT_OFFSET = 1; // I don't know why the text is located at 1px to the right. Maybe it's only for me.

            ListView listView = (ListView) sender;

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

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _myContextMenu.Show(listView1, e.Location, LeftRightAlignment.Right);
        }

        private void InitContextmenu()
        {
            _myContextMenu = new ContextMenu();

            MenuItem menuItemVS = new MenuItem("Open Visual Studio");
            MenuItem menuItem2 = new MenuItem("Delete");
            MenuItem menuItem3 = new MenuItem("Add quantity");

            _myContextMenu.MenuItems.Add(menuItemVS);
            _myContextMenu.MenuItems.Add(menuItem2);
            _myContextMenu.MenuItems.Add(menuItem3);
            //ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;

            //myContextMenu.MenuItems[0].Visible = true;
            //myContextMenu.MenuItems[1].Visible = false;
            //myContextMenu.MenuItems[2].Visible = true;

            menuItemVS.Click += VS_Click;
        }

        private void VS_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
            CmdCommand.OpenVisualStudio(displayItem[indexes[0]].Path);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection indexes = listView1.SelectedIndices;
            CmdCommand.SystemDefaultOpen(displayItem[indexes[0]].Path);
        }
    }
}