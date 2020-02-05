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

namespace DupesMaintWinForms.Forms
{
    public partial class frmDupesAction : Form
    {
        private string theFileName;

        public frmDupesAction()
        {
            InitializeComponent();
        }


        private void frmDupesAction_Load(object sender, EventArgs e)
        {
            this.dataGridView.AutoGenerateColumns = true;
            this.dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Program.popsModel.DupesActions.Where(x => x.GooglePhotosRemoved=="N").Load();
            this.popsDataSetBindingSource.DataSource = Program.popsModel.DupesActions.Local.ToBindingList();

            // hide some columns
            this.dataGridView.Columns[1].Visible = false;
            this.dataGridView.Columns[2].Visible = false;
            this.dataGridView.Columns["FileExt"].Visible = false;

            // auto column width
            this.dataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView.Columns["GooglePhotosRemoved"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            this.dataGridView.Rows[0].Selected = true;

            this.toolStripStatusLabel1.Text = $"INFO - Loaded {this.dataGridView.RowCount} dupesActions rows with GooglePhotosRemoved = N";
        }


        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dataGridView.SelectedRows.Count > 0)
            {
                // get the TheFilename from the selected grid row
                theFileName = dataGridView.SelectedRows[0].Cells[0].Value.ToString();

                string dupesPhotoPath = theFileName.Replace("C:\\", Program.targetRootFolder);

                FileInfo fileInfo = new FileInfo(dupesPhotoPath);
                if (fileInfo.Exists)
                {
                    this.pictureBox.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(dupesPhotoPath)));
                } 
                else
                {
                    this.toolStripStatusLabel1.Text = $"ERROR - File not found {dupesPhotoPath}";
                }

            }

        }

        private void btnBinned_Click(object sender, EventArgs e)
        {
            if (this.dataGridView.SelectedRows.Count == 1)
            {
                // get the DupesAction row with the selected TheFileName
                IQueryable<DupesAction> query = Program.popsModel.DupesActions.Where(dupesAction => dupesAction.TheFileName == theFileName);

                DupesAction adupesAction = query.First();

                // if 1 row found then update GooglePhotosRemoved = Y and save the change to the DB
                if (!string.IsNullOrEmpty(adupesAction.TheFileName))
                {
                    // update the db table
                    adupesAction.GooglePhotosRemoved = "Y";
                    Program.popsModel.SaveChanges();

                    // remove the binned row from the datagrid
                    this.dataGridView.Rows.Remove(this.dataGridView.SelectedRows[0]);

                    // make the first row selected
                    this.dataGridView.Rows[0].Selected = true;

                    this.toolStripStatusLabel1.Text = $"INFO - {this.dataGridView.RowCount} dupesActions rows with GooglePhotosRemoved = N";

                }
                else
                {
                    this.toolStripStatusLabel1.Text = $"ERROR - DupesAction row {theFileName} was not found.";
                }

            }
        }

    }
}
