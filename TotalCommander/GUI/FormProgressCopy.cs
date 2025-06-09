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

        public FormProgressCopy(string[] sourceFiles, string destPath, bool cut)
        {
            InitializeComponent();
            files = sourceFiles;
            destinationPath = destPath;
            isCut = cut;
            totalFiles = files.Length;

            // 복사할 총 바이트 수 계산
            CalculateTotalBytes();
            
            // Load 이벤트 핸들러 등록
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
            this.lblTitle.Text = "파일 복사";
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
            this.lblStatus.Text = "준비 중...";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(164, 110);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "취소";
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
            this.Text = "파일 복사";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnCancel;

        private bool isCancelled = false;

        /// <summary>
        /// 작업이 취소되었는지 여부
        /// </summary>
        public bool IsCancelled
        {
            get { return isCancelled; }
        }

        /// <summary>
        /// 진행률 설정 (0-100)
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
        /// 상태 메시지 설정
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
            lblStatus.Text = "취소 중...";
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
            
            // 파일 크기 합산
            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                size += fileInfo.Length;
            }

            // 하위 디렉토리 크기 합산
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                size += GetDirectorySize(subDirInfo.FullName);
            }

            return size;
        }

        private async void FormProgressCopy_Load(object sender, EventArgs e)
        {
            lblStatus.Text = isCut ? "파일 이동 중..." : "파일 복사 중...";
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            await Task.Run(() => ProcessFiles());
            this.Close();
        }

        private void ProcessFiles()
        {
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
                        UpdateUI($"{(isCut ? "이동" : "복사")} 중: {fileName}", fileName);
                        PasteFile(source, target, UIOption.AllDialogs, UICancelOption.ThrowException);
                        UpdateProgress(new FileInfo(source).Length);
                    }
                    else if (Directory.Exists(source))
                    {
                        UpdateUI($"{(isCut ? "이동" : "복사")} 중: {fileName}", fileName);
                        PasteDir(source, target, UIOption.AllDialogs, UICancelOption.ThrowException);
                        UpdateProgress(GetDirectorySize(source));
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show(this, $"오류: {ex.Message}", "작업 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    else
                    {
                        MessageBox.Show(this, $"오류: {ex.Message}", "작업 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                completedFiles++;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = cancelRequested ? "작업이 취소되었습니다." : "작업이 완료되었습니다.";
                    lblStatus.Text = $"완료: {completedFiles}/{totalFiles} 파일";
                }));
            }
            else
            {
                lblStatus.Text = cancelRequested ? "작업이 취소되었습니다." : "작업이 완료되었습니다.";
                lblStatus.Text = $"완료: {completedFiles}/{totalFiles} 파일";
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
                    lblStatus.Text = $"완료: {completedFiles + 1}/{totalFiles} 파일, " +
                                      $"{ShellBrowser.FormatBytes(copiedBytes)}/{ShellBrowser.FormatBytes(totalBytes)}";
                }));
            }
            else
            {
                progressBar.Value = Math.Min(progressPercentage, 100);
                lblStatus.Text = $"완료: {completedFiles + 1}/{totalFiles} 파일, " +
                                  $"{ShellBrowser.FormatBytes(copiedBytes)}/{ShellBrowser.FormatBytes(totalBytes)}";
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