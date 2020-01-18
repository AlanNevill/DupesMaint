using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace DupesMaintWinForms
{
    public partial class DisplayPhotos4SHA : Form
    {

        public popsDataSet.CheckSumDataTable _dupes { get; set; }

        public CheckSum[] CheckSums { get; set; }
        public CheckSum Photo1 { get; set; }
        public CheckSum Photo2 { get; set; }
        public CheckSumDup[] checkSumDups { get; set; }
        public CheckSumDup checkSumDup1 { get; set; }
        public CheckSumDup checkSumDup2 { get; set; }

        public DisplayPhotos4SHA()
        {
            InitializeComponent();
        }


        // constructor called by form SelectbySHA passing in the SHA string of the selected duplicates
        public DisplayPhotos4SHA(string SHA)
        {
            InitializeComponent();
 
            // query the model for the CheckSum rows with the selected SHA string
            IQueryable<CheckSum> query = Program.popsModel.CheckSums.Where(checkSum => checkSum.SHA == SHA).OrderBy(x => x.Id);

            // cast the query to an array of CheckSum rows
            this.CheckSums = query.ToArray();
            Photo1 = CheckSums[0];
            Photo2 = CheckSums[1];
            this.toolStripStatusLabel.Text = $"INFO - {this.CheckSums.Length} duplicate photos - {SHA}";

            // get the CheckSumDup rows from the db for the 2 photos
            IQueryable<CheckSumDup> query2 = Program.popsModel.CheckSumDups.Where(a => a.Id == Photo1.Id || a.Id == Photo2.Id).OrderBy(b => b.Id);
            this.checkSumDups = query2.ToArray();
            
            if (this.checkSumDups.Length==1)
            {
                this.toolStripStatusLabel.Text = "ERROR - Only 1 photo found for this SHA value.";
                return;
            }

            this.checkSumDup1 = checkSumDups[0];
            this.checkSumDup2 = checkSumDups[1];


            // Note the escape character used (@) when specifying the path. 
            try
            {
                using (MemoryStream stream1 = new MemoryStream(File.ReadAllBytes(@Photo1.TheFileName)))
                {
                    this.pictureBox1.Image = Image.FromStream(stream1);
                    stream1.Dispose();
                }
                using (MemoryStream stream2 = new MemoryStream(File.ReadAllBytes(@Photo2.TheFileName)))
                {
                    this.pictureBox2.Image = Image.FromStream(stream2);
                    stream2.Dispose();
                }

                //this.pictureBox1.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(@Photo1.TheFileName)));
                //this.pictureBox2.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(@Photo2.TheFileName)));
            }
            catch (Exception e)
            {
                MessageBox.Show($"ERROR\n\r{e.ToString()}");
                this.Close();
            }

            this.tbPhoto1.Text = Photo1.TheFileName;
            this.tbPhoto2.Text = Photo2.TheFileName;

            this.dateTimePhoto1.Format = DateTimePickerFormat.Custom;
            this.dateTimePhoto2.Format = DateTimePickerFormat.Custom;
            this.dateTimePhoto1.CustomFormat = "yyyy-MM-dd hh:mm:ss";
            this.dateTimePhoto2.CustomFormat = "yyyy-MM-dd hh:mm:ss";
            this.dateTimePhoto1.Value = Photo1.FileCreateDt;
            this.dateTimePhoto2.Value = Photo2.FileCreateDt;

            this.cbPhoto1.Text = $"Move photo1 with Id {Photo1.Id.ToString()}";
            this.cbPhoto2.Text = $"Move photo2 with Id {Photo2.Id.ToString()}";

            this.toolStripStatusLabel.Text = $"INFO - {this.CheckSums.Length} photos found for this SHA value.";
        }


        // no longer used
        // called from constructor to load the CheckSum photos for the selected SHA into picture boxes ?? how many
        private void LoadCheckSumPhoto()
        {
            popsDataSet.CheckSumRow photo1 = (popsDataSet.CheckSumRow)_dupes.Rows[0];
            popsDataSet.CheckSumRow photo2 = (popsDataSet.CheckSumRow)_dupes.Rows[1];

            // Note the escape character used (@) when specifying the path.  
            pictureBox1.Image = Image.FromFile(@photo1.TheFileName);
            pictureBox2.Image = Image.FromFile(@photo2.TheFileName);

            tbPhoto1.Text = photo1.TheFileName;
            tbPhoto2.Text = photo2.TheFileName;

            dateTimePhoto1.Format = DateTimePickerFormat.Custom;
            dateTimePhoto2.Format = DateTimePickerFormat.Custom;
            dateTimePhoto1.CustomFormat = "yyyy-MM-dd hh:mm:ss";
            dateTimePhoto2.CustomFormat = "yyyy-MM-dd hh:mm:ss";
            dateTimePhoto1.Value = photo1.FileCreateDt;
            dateTimePhoto2.Value = photo2.FileCreateDt;
        }


        private void cbPhoto1_CheckedChanged(object sender, EventArgs e)
        {
            // move Photo1 in file system from OneDrive Photos folder to target root folder
            if (!PhotoMove(Photo1))
            {
                this.toolStripStatusLabel.Text = $"ERROR - Photo1.id {Photo1.Id} was not moved.";
                return;
            }

            // if move succeeds then write a new DupesAction row for Photo1
            DupesAction_Insert(Photo1, Photo2.TheFileName );

            // delete Photo1 row from CheckSum table and delete Photo1 and Photo2 from CheckSumDups
            Db_Delete(Photo1, checkSumDup1, checkSumDup2);

            // close the form and SelectBySHA form will refresh without the 2 CheckSumDup rows
            this.Close();

        }

        private void cbPhoto2_CheckedChanged(object sender, EventArgs e)
        {
            // move Photo2 in file system from OneDrive Photos folder to target root folder
            if (!PhotoMove(Photo2))
            {
                this.toolStripStatusLabel.Text = $"ERROR - Photo2.id {Photo2.Id} was not moved.";
                return;
            }

            // if move succeeds then write a new DupesAction row for Photo1
            DupesAction_Insert(Photo2, Photo1.TheFileName );

            // delete Photo1 row from CheckSum table and delete Photo1 and Photo2 from CheckSumDups
            Db_Delete(Photo2, checkSumDup1, checkSumDup2);

            // close the form and SelectBySHA form will refresh without the 2 CheckSumDup rows
            this.Close();

        }


        // write new row into the DupesAction table
        private void DupesAction_Insert(CheckSum photo, string duplicateOf)
        {
            // create a new DupesAction row
            DupesAction dupesAction = new DupesAction();

            dupesAction.TheFileName = photo.TheFileName;
            dupesAction.DuplicateOf = duplicateOf;
            dupesAction.SHA = photo.SHA;
            dupesAction.FileExt = photo.FileExt;
            dupesAction.FileSize = photo.FileSize;
            dupesAction.FileCreateDt = photo.FileCreateDt;
            dupesAction.OneDriveRemoved = "Y";
            dupesAction.GooglePhotosRemoved = "N";

            // call the custom stored procedure method in DbContext popsModel
            Program.popsModel.DupesAction_ins(dupesAction);
        }


        // Move the file specified in CheckSum photo to a new directory
        private bool PhotoMove(CheckSum photo)
        {
            // check if target folder exists, if not create it
            string targetPath = TargetFolderCheck(photo);

            // now move the file from source folder to target folder
            return PhotoMove(photo, targetPath);
        }


        private static string TargetFolderCheck(CheckSum photo)
        {
            string targetFolder = Program.targetRootFolder;

            // make sure the targetRootFolder exists but only need to check once
            if (!Program.targetRootFolderExists)
            {
                DirectoryInfo diRoot = new DirectoryInfo(Program.targetRootFolder);
                if (!diRoot.Exists)
                {
                    diRoot.Create();
                }
                Program.targetRootFolderExists = true;
            }

            // construct the targetFolder for this CheckSum photo
            targetFolder += @photo.Folder.Substring(2);

            // if target folder does not exist then create it
            DirectoryInfo diTarget = new DirectoryInfo(targetFolder);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            Console.WriteLine($"INFO - Target folder {photo.Folder} exists under {Program.targetRootFolder}.");

            return targetFolder;
        }


        // Physically move the file from its source location to the target folder
        private static bool PhotoMove(CheckSum photo, string targetPath)
        {
            // construct the destPath including the file name
            string[] sourceFolderParts = photo.TheFileName.Split('\\');
            string fileName = sourceFolderParts[sourceFolderParts.Length - 1];
            string destPath = targetPath + "\\" + fileName;

            // instaniate a FileInfo object for the source file
            FileInfo sourcePath = new FileInfo(photo.TheFileName);
            try
            {
                // move the file from sourcePath to destPath
                sourcePath.MoveTo(destPath);
                Console.WriteLine($"INFO - File {photo.TheFileName} was moved to {destPath}.");

                return true;
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"ERROR - File {photo.TheFileName} was NOT moved. See console.");

                Program.DisplayException(Ex);
                return false;
            }
        }


        // delete the CheckSum row that was moved so that CheckSum still reflects the folder scan, and remove the 2 CheckSumDups rows as the duplicate has been removed
        private void Db_Delete(CheckSum photo1, CheckSumDup checkSumDup1, CheckSumDup checkSumDup2)
        {
            Program.popsModel.CheckSums.Remove(photo1);
            Program.popsModel.CheckSumDups.Remove(checkSumDup1);
            Program.popsModel.CheckSumDups.Remove(checkSumDup2);
            Program.popsModel.SaveChanges();
        }

        private void DisplayPhotos4SHA_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.pictureBox1.Dispose();
            this.pictureBox2.Dispose();

            GC.Collect();
        }
    }
}
