using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
using TotalCommander;

namespace TotalCommander.GUI
{
    public partial class FormProgressCopy : Form
    {
        private string[] files;
        private string destinationPath;
        private bool isCut;
        private int totalFiles;
        private int completedFiles = 0;
        private long totalBytes = 0;
        private long copiedBytes = 0;
        private bool cancelRequested = false;

        // 작업 완료 이벤트 정의
        public event EventHandler OperationCompleted;

        public FormProgressCopy(string[] sourceFiles, string destPath, bool cut)
        {
            InitializeComponent();
            files = sourceFiles;
            destinationPath = destPath;
            isCut = cut;
            totalFiles = files.Length;

            // Calculate total bytes to copy
            CalculateTotalBytes();
            
            // 커스텀 진행 대화 상자를 표시하지 않음
            this.Visible = false;
            
            // Register Load event handler
            this.Load += FormProgressCopy_Load;
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(74, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = StringResources.GetString("FileCopyTitle");
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(16, 42);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(372, 23);
            this.progressBar.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Location = new System.Drawing.Point(16, 76);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(372, 23);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = StringResources.GetString("Preparing");
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(164, 110);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = StringResources.GetString("Cancel");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormProgressCopy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 145);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProgressCopy";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = StringResources.GetString("FileCopyTitle");
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnCancel;

        private bool isCancelled = false;

        /// <summary>
        /// Whether the operation was cancelled
        /// </summary>
        public bool IsCancelled
        {
            get { return isCancelled; }
        }

        /// <summary>
        /// Set progress (0-100)
        /// </summary>
        public void SetProgress(int percent)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(SetProgress), percent);
                return;
            }

            progressBar.Value = percent;
        }

        /// <summary>
        /// Set status message
        /// </summary>
        public void SetStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetStatus), message);
                return;
            }

            lblStatus.Text = message;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            isCancelled = true;
            btnCancel.Enabled = false;
            lblStatus.Text = StringResources.GetString("Cancelling");
        }

        private void CalculateTotalBytes()
        {
            totalBytes = 0;
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    totalBytes += fileInfo.Length;
                }
                else if (Directory.Exists(file))
                {
                    totalBytes += GetDirectorySize(file);
                }
            }
        }

        private long GetDirectorySize(string directoryPath)
        {
            long size = 0;
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
            
            // Calculate file sizes
            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                size += fileInfo.Length;
            }

            // Calculate subdirectory sizes
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                size += GetDirectorySize(subDirInfo.FullName);
            }

            return size;
        }

        /// <summary>
        /// 대화 상자를 표시하지 않고 백그라운드에서 파일 처리를 수행합니다.
        /// </summary>
        public void ProcessFilesInBackground()
        {
            // 폼 초기화
            lblStatus.Text = isCut ? StringResources.GetString("FileMoving") : StringResources.GetString("FileCopying");
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            
            // 백그라운드에서 파일 처리 실행
            Task.Run(() => ProcessFiles());
        }

        private async void FormProgressCopy_Load(object sender, EventArgs e)
        {
            lblStatus.Text = isCut ? StringResources.GetString("FileMoving") : StringResources.GetString("FileCopying");
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            await Task.Run(() => ProcessFiles());
            this.Close();
        }

        private void ProcessFiles()
        {
            try
            {
                if (files == null || files.Length == 0)
                {
                    UpdateUI(StringResources.GetString("OperationError"), StringResources.GetString("NoFilesToProcess"));
                    return;
                }

                PasteFileDel PasteFile;
                PasteDirDel PasteDir;

                if (isCut)
                {
                    PasteFile = FileSystem.MoveFile;
                    PasteDir = FileSystem.MoveDirectory;
                }
                else
                {
                    PasteFile = FileSystem.CopyFile;
                    PasteDir = FileSystem.CopyDirectory;
                }

                foreach (string source in files)
                {
                    if (cancelRequested)
                        break;

                    string fileName = Path.GetFileName(source);
                    string target = Path.Combine(destinationPath, fileName);

                    try
                    {
                        if (File.Exists(source))
                        {
                            // Check if source and target are in the same directory
                            bool isSameDirectory = Path.GetDirectoryName(source).Equals(destinationPath, StringComparison.OrdinalIgnoreCase);
                            
                            // If copying to the same directory, create a new filename with suffix
                            if (isSameDirectory && !isCut)
                            {
                                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                                string fileExt = Path.GetExtension(fileName);
                                
                                // Generate a unique filename by adding _(1), _(2), etc.
                                int counter = 1;
                                while (File.Exists(target))
                                {
                                    target = Path.Combine(destinationPath, $"{fileNameWithoutExt}_({counter}){fileExt}");
                                    counter++;
                                }
                            }

                            FileInfo sourceInfo = new FileInfo(source);
                            UpdateUI(isCut ? StringResources.GetString("FileMoving") : StringResources.GetString("FileCopying"), sourceInfo.Name);

                            // 시스템 대화 상자 표시
                            PasteFile(source, target, UIOption.AllDialogs, UICancelOption.DoNothing);
                            
                            UpdateProgress(sourceInfo.Length);
                        }
                        else if (Directory.Exists(source))
                        {
                            // Check if source and target are in the same directory
                            bool isSameDirectory = Path.GetDirectoryName(source).Equals(destinationPath, StringComparison.OrdinalIgnoreCase);
                            
                            // If copying to the same directory, create a new directory name with suffix
                            if (isSameDirectory && !isCut)
                            {
                                string dirName = Path.GetFileName(source);
                                
                                // Generate a unique directory name by adding _(1), _(2), etc.
                                int counter = 1;
                                while (Directory.Exists(target))
                                {
                                    target = Path.Combine(destinationPath, $"{dirName}_({counter})");
                                    counter++;
                                }
                            }

                            DirectoryInfo dirInfo = new DirectoryInfo(source);
                            UpdateUI(isCut ? StringResources.GetString("FolderMoving") : StringResources.GetString("FolderCopying"), dirInfo.Name);

                            // 시스템 대화 상자 표시
                            PasteDir(source, target, UIOption.AllDialogs, UICancelOption.DoNothing);
                            
                            long dirSize = GetDirectorySize(source);
                            UpdateProgress(dirSize);
                        }

                        completedFiles++;
                    }
                    catch (Exception ex)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                CustomDialogHelper.ShowMessageBox(this, StringResources.GetString("ErrorMessage", ex.Message),
                                    StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        }
                        else
                        {
                            CustomDialogHelper.ShowMessageBox(this, StringResources.GetString("ErrorMessage", ex.Message),
                                StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                if (completedFiles == totalFiles)
                {
                    // 작업 완료, 부모 폼에 알림
                    UpdateUI(StringResources.GetString("OperationCompleted"), StringResources.GetString("CompletedFiles", completedFiles, totalFiles));
                    
                    // 작업 완료 이벤트 발생
                    OperationCompleted?.Invoke(this, EventArgs.Empty);
                }
                else if (isCancelled)
                {
                    UpdateUI(StringResources.GetString("OperationCancelled"), "");
                }
                else
                {
                    UpdateUI(StringResources.GetString("OperationError"), "");
                }
            }
            catch (Exception ex)
            {
                UpdateUI(StringResources.GetString("OperationError"), StringResources.GetString("ErrorMessage", ex.Message));
            }
        }

        private void UpdateProgress(long processedBytes)
        {
            copiedBytes += processedBytes;
            int progressPercentage = totalBytes > 0 ? (int)((copiedBytes * 100) / totalBytes) : 100;
            
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    progressBar.Value = Math.Min(progressPercentage, 100);
                    lblStatus.Text = StringResources.GetString("CompletedFilesPercent", 
                        completedFiles + 1, totalFiles, 
                        $"{ShellBrowser.FormatBytes(copiedBytes)}/{ShellBrowser.FormatBytes(totalBytes)}");
                }));
            }
            else
            {
                progressBar.Value = Math.Min(progressPercentage, 100);
                lblStatus.Text = StringResources.GetString("CompletedFilesPercent", 
                    completedFiles + 1, totalFiles, 
                    $"{ShellBrowser.FormatBytes(copiedBytes)}/{ShellBrowser.FormatBytes(totalBytes)}");
            }
        }

        private void UpdateUI(string status, string detail)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lblTitle.Text = status;
                    lblStatus.Text = detail;
                }));
            }
            else
            {
                lblTitle.Text = status;
                lblStatus.Text = detail;
            }
        }

        private delegate void PasteFileDel(string source, string dest, UIOption uiOption, UICancelOption cancelOption);
        private delegate void PasteDirDel(string source, string dest, UIOption uiOption, UICancelOption cancelOption);
    }
} 