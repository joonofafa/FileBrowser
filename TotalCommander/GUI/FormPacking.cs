using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TotalCommander;

namespace TotalCommander.GUI
{
    public partial class FormPacking : Form
    {
        #region Constants

        // Compression level enum
        private enum CompressionLevel
        {
            NoCompression = 0,
            Fastest = 1,
            Optimal = 9,
            SmallestSize = 9
        }

        // Update mode enum
        private enum UpdateMode
        {
            Add = 0,
            Update = 1,
            Fresh = 2,
            Sync = 3
        }

        #endregion

        #region Fields

        public string[] FileList { get; set; }
        public string DestArchive { get; set; }
        
        #endregion

        #region Constructor

        public FormPacking()
        {
            InitializeComponent();
            FormClosed += (s, e) => { timerProgress.Stop(); };
            btnOpenSaveDialog.Click += btnArchiveOpenFolder_Click;
            btnOK.Click += btnAdd_Click;
            btnCancel.Click += btnCancel_Click;
            FormClosing += FormPacking_FormClosing;
            Load += FormPacking_Load;
        }

        public FormPacking(string[] filePaths) : this()
        {
            FileList = filePaths;
            if (filePaths.Length > 0)
            {
                // Set default archive name (create in the directory of the first file)
                string directory = Path.GetDirectoryName(filePaths[0]);
                DestArchive = Path.Combine(directory, "Archive.zip");
            }
        }

        #endregion

        #region Form Events

        private void FormPacking_Load(object sender, EventArgs e)
        {
            txtFileName.Text = FileList != null ? string.Join(Environment.NewLine, FileList) : string.Empty;
            txtFileName.Text = DestArchive != null ? DestArchive : string.Empty;

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            
            InitCompressionLevelComboBox();
            InitArchiveFormatComboBox();
            InitUpdateModeComboBox();
            
            // Initialize UI
            progressBar1.Visible = false;
            lblStatus.Visible = false;
            timerProgress.Enabled = false;
        }

        private void FormPacking_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerProgress.Stop();
        }

        private void btnArchiveOpenFolder_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = "archive.zip";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFileName.Text = saveFileDialog.FileName;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show(
                    StringResources.GetString("ZipFileRequired"), 
                    StringResources.GetString("Warning"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            if (FileList == null || FileList.Length == 0)
            {
                MessageBox.Show(
                    StringResources.GetString("NoFilesToCompress"), 
                    StringResources.GetString("Warning"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                    return;
            }

            // Set compression options
            CompressionLevel level = (CompressionLevel)((ComboBoxItem)cboCompressionLevel.SelectedItem).Value;
            
            // Set update mode
            UpdateMode updateMode = (UpdateMode)((ComboBoxItem)cboUpdateMode.SelectedItem).Value;

            // Update UI
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            lblStatus.Text = StringResources.GetString("Compressing");
            lblStatus.Visible = true;
            timerProgress.Start();
            
            // Start compression
            CompressFiles(txtFileName.Text, FileList, level, updateMode);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            timerProgress.Stop();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region Compression Methods

        private void CompressFiles(string archivePath, string[] files, CompressionLevel level, UpdateMode updateMode)
        {
            try
            {
                // Perform compression in background
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += (s, e) => 
                {
                    try
                    {
                        // Use .NET compression
                        bool fileExists = File.Exists(archivePath);
                        
                        // Process according to update mode
                        if (fileExists && updateMode == UpdateMode.Fresh)
                        {
                            File.Delete(archivePath);
                            fileExists = false;
                        }
                        
                        if (!fileExists)
                        {
                            // Create new ZIP file
                            using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                            {
                                int count = 0;
                                foreach (string file in files)
                                {
                                    if (File.Exists(file))
                                    {
                                        string entryName = Path.GetFileName(file);
                                        ZipArchiveEntry entry = archive.CreateEntry(entryName, 
                                            (System.IO.Compression.CompressionLevel)level);
                                        
                                        using (Stream entryStream = entry.Open())
                                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                                        {
                                            fs.CopyTo(entryStream);
                                        }
                                        
                                        count++;
                                        int progress = (int)((count / (double)files.Length) * 100);
                                        worker.ReportProgress(progress);
                                    }
                                }
                            }
                }
                else
                {
                            // Update existing ZIP file
                            using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
                            {
                                int count = 0;
                                foreach (string file in files)
                                {
                                    if (File.Exists(file))
                                    {
                                        string entryName = Path.GetFileName(file);
                                        
                                        // Remove existing entry (if in update mode)
                                        ZipArchiveEntry existingEntry = archive.GetEntry(entryName);
                                        if (existingEntry != null)
                                        {
                                            existingEntry.Delete();
                                        }
                                        
                                        // Add new entry
                                        ZipArchiveEntry entry = archive.CreateEntry(entryName, 
                                            (System.IO.Compression.CompressionLevel)level);
                                        
                                        using (Stream entryStream = entry.Open())
                                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                                        {
                                            fs.CopyTo(entryStream);
                                        }
                                        
                                        count++;
                                        int progress = (int)((count / (double)files.Length) * 100);
                                        worker.ReportProgress(progress);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Result = ex;
                    }
                };
                
                worker.ProgressChanged += (s, e) =>
                {
                    // Update UI
                    progressBar1.Value = e.ProgressPercentage;
                    lblStatus.Text = StringResources.GetString("CompressionProgress", e.ProgressPercentage);
                };
                
                worker.RunWorkerCompleted += (s, e) =>
                {
                    timerProgress.Stop();
                    
                    if (e.Result is Exception ex)
                    {
                        MessageBox.Show(
                            StringResources.GetString("CompressionErrorMessage", ex.Message), 
                            StringResources.GetString("CompressionError"), 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                        progressBar1.Visible = false;
                        lblStatus.Visible = false;
                    }
                    else
                    {
                        progressBar1.Value = 100;
                        lblStatus.Text = StringResources.GetString("CompressionComplete");
                        MessageBox.Show(
                            StringResources.GetString("CompressionCompleteMessage"), 
                            StringResources.GetString("Information"), 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                };
                
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    StringResources.GetString("CompressionStartErrorMessage", ex.Message), 
                    StringResources.GetString("CompressionStartError"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                progressBar1.Visible = false;
                lblStatus.Visible = false;
                timerProgress.Stop();
            }
        }

        #endregion

        #region UI Initialization Methods

        private void InitCompressionLevelComboBox()
        {
            cboCompressionLevel.Items.Clear();
            
            ComboBoxItem[] arrItems = new ComboBoxItem[3];
            
            // Add compression level options
            ComboBoxItem item = new ComboBoxItem()
            {
                Text = StringResources.GetString("FastCompression"),
                Value = (int)CompressionLevel.Fastest
            };
            arrItems[0] = item;

            item = new ComboBoxItem()
            {
                Text = StringResources.GetString("NormalCompression"),
                Value = (int)CompressionLevel.Optimal
            };
            arrItems[1] = item;

            item = new ComboBoxItem()
            {
                Text = StringResources.GetString("MaximumCompression"),
                Value = (int)CompressionLevel.SmallestSize
            };
            arrItems[2] = item;
            
            cboCompressionLevel.Items.AddRange(arrItems);
            cboCompressionLevel.SelectedIndex = 1; // Default: Normal
        }

        private void InitArchiveFormatComboBox()
        {
            cboArchiveFormat.Items.Clear();
            
            // Only ZIP format is supported
            ComboBoxItem item = new ComboBoxItem()
            {
                Text = "ZIP",
                Value = 0
            };
            cboArchiveFormat.Items.Add(item);
            cboArchiveFormat.SelectedIndex = 0;
            cboArchiveFormat.Enabled = false; // Currently only ZIP is supported, so disable
        }

        private void InitUpdateModeComboBox()
        {
            cboUpdateMode.Items.Clear();
            
            ComboBoxItem[] cboItems = new ComboBoxItem[3];

            ComboBoxItem item = new ComboBoxItem()
            {
                Text = StringResources.GetString("AddToExistingArchive"),
                Value = (int)UpdateMode.Add
            };
            cboItems[0] = item;

            item = new ComboBoxItem()
            {
                Text = StringResources.GetString("UpdateExistingFiles"),
                Value = (int)UpdateMode.Update
            };
            cboItems[1] = item;

            item = new ComboBoxItem()
            {
                Text = StringResources.GetString("CreateNewArchive"),
                Value = (int)UpdateMode.Fresh
            };
            cboItems[2] = item;

            cboUpdateMode.Items.AddRange(cboItems);
            cboUpdateMode.SelectedIndex = 0; // Default: Add
        }

        #endregion

        // Timer event handler
        private void timerProgress_Tick(object sender, EventArgs e)
        {
            // Actual progress is updated from the background worker
        }
    }
}
