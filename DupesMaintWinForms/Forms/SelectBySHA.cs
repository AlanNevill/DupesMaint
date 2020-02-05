using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DupesMaintWinForms
{
    public partial class SelectBySHA : Form
    {
        private string _fileExt = ".*";

        public SelectBySHA()
        {
            InitializeComponent();
        }

        private void SelectBySHA_Load(object sender, EventArgs e)
        {
            this.dataGridView1.AutoGenerateColumns = true;

            SHAgrid_Load();

        }

        private void SHAgrid_Load()
        {
            Program.popsModel.CheckSumDups.Local.Clear();

            if (_fileExt == ".*")
            {
                Program.popsModel.CheckSumDups.OrderBy(x => x.SHA).Load();
            }
            else
            {
                Program.popsModel.CheckSumDups.Where(a => a.FileExt == this._fileExt).Load();
            }

            // use ToBindingList() so that the DbContext keeps track of table updates and then datagridview refresh will reflect the deletes made in the form DisplayPhotos4SHA
            this.checkSumDupsBindingSource.DataSource = Program.popsModel.CheckSumDups.Local.ToBindingList();

            // sort the datagridview by SHA
            // this.dataGridView1.Sort(this.dataGridView1.Columns[1], ListSortDirection.Ascending);

            // auto column width
            this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns["SHA"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns["ToDelete"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            this.toolStripStatusLabel1.Text = $"INFO - {Program.popsModel.CheckSumDups.Count()} CheckSumDups rows loaded.";
        }


        // 
        private void btnTest_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"ERROR - NOT ENABLED.");
        }


        // when grid selected row changes call the DisplayPhotos4SHA form passing in the SHA value
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count == 0)
            {
                return;
            }

            string _fileExt = this.dataGridView1.SelectedRows[0].Cells["FileExt"].Value.ToString();
            _fileExt = _fileExt.Substring(startIndex: _fileExt.Length - 4);

            if (!_fileExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !_fileExt.Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"ERROR - File type {_fileExt} cannot be displayed.");
                return;
            }

            // get the SHA value from the selected grid row
            string SHA = this.dataGridView1.SelectedRows[0].Cells[1].Value.ToString();

            // call DisplayPhotos4SHA passing in the SHA of the selected duplicates
            Form displayPhotos4SHA = new DisplayPhotos4SHA(SHA);
                
            if (!displayPhotos4SHA.IsDisposed)
            {
                displayPhotos4SHA.ShowDialog();
            }

            // refresh the datagrid in case DisplayPhotos4SHA has deleted 2 CheckSumDups rows
            this.dataGridView1.Refresh();
            this.toolStripStatusLabel1.Text = $"{this.dataGridView1.RowCount} CheckSumDups rows.";
        }


        // call the DupesAction form
        private void menuFile_Click(object sender, EventArgs e)
        {
            Form frmdDupesAction = new Forms.frmDupesAction();
            frmdDupesAction.ShowDialog();
        }


       // user changes the selected FileExt type
       private void cbFileExt_SelectedIndexChanged(object sender, EventArgs e)
        {
            _fileExt = this.cbFileExt.SelectedItem.ToString();
            SHAgrid_Load();
        }

    }
}
