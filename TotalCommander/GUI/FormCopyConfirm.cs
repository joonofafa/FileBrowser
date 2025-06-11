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
using TotalCommander;

namespace TotalCommander.GUI
{
    public partial class FormCopyConfirm : Form
    {
        /// <summary>
        /// 복사할 파일 수
        /// </summary>
        public int FileCount { get; private set; }

        /// <summary>
        /// 복사할 총 용량
        /// </summary>
        public long TotalSize { get; private set; }
        
        /// <summary>
        /// 원본 경로
        /// </summary>
        public string SourcePath { get; private set; }
        
        /// <summary>
        /// 대상 경로
        /// </summary>
        public string DestinationPath { get; private set; }

        public FormCopyConfirm(string[] files, string destPath)
        {
            InitializeComponent();
            
            // 파일 목록 설정
            FileCount = files.Length;
            SourcePath = Path.GetDirectoryName(files.FirstOrDefault() ?? string.Empty);
            DestinationPath = destPath;
            
            // 전체 크기 계산
            TotalSize = CalculateTotalSize(files);
            
            // UI 업데이트
            UpdateUI();
        }
        
        /// <summary>
        /// 총 크기 계산
        /// </summary>
        private long CalculateTotalSize(string[] files)
        {
            long totalSize = 0;
            
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
                else if (Directory.Exists(file))
                {
                    totalSize += GetDirectorySize(file);
                }
            }
            
            return totalSize;
        }
        
        /// <summary>
        /// 디렉토리 크기 계산
        /// </summary>
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
        
        /// <summary>
        /// UI 업데이트
        /// </summary>
        private void UpdateUI()
        {
            // 경로 정보 표시
            lblSourcePath.Text = StringResources.GetString("SourcePath") + SourcePath;
            lblDestPath.Text = StringResources.GetString("DestinationPath") + DestinationPath;
            
            // 파일 수 표시
            lblFileCount.Text = StringResources.GetString("FileCount", FileCount);
            
            // 용량 표시
            lblTotalSize.Text = StringResources.GetString("CopySize", FormatBytes(TotalSize));
        }
        
        /// <summary>
        /// 바이트 크기를 사람이 읽기 쉬운 형식으로 변환
        /// </summary>
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            
            return string.Format("{0:n2} {1}", number, suffixes[counter]);
        }

        private void InitializeComponent()
        {
            this.lblSourcePath = new System.Windows.Forms.Label();
            this.lblDestPath = new System.Windows.Forms.Label();
            this.lblFileCount = new System.Windows.Forms.Label();
            this.lblTotalSize = new System.Windows.Forms.Label();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblSeparator = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSourcePath
            // 
            this.lblSourcePath.AutoSize = true;
            this.lblSourcePath.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSourcePath.Location = new System.Drawing.Point(12, 12);
            this.lblSourcePath.Name = "lblSourcePath";
            this.lblSourcePath.Size = new System.Drawing.Size(53, 12);
            this.lblSourcePath.TabIndex = 0;
            this.lblSourcePath.Text = "Source: ";
            // 
            // lblDestPath
            // 
            this.lblDestPath.AutoSize = true;
            this.lblDestPath.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDestPath.Location = new System.Drawing.Point(12, 32);
            this.lblDestPath.Name = "lblDestPath";
            this.lblDestPath.Size = new System.Drawing.Size(71, 12);
            this.lblDestPath.TabIndex = 1;
            this.lblDestPath.Text = "Destination:";
            // 
            // lblFileCount
            // 
            this.lblFileCount.AutoSize = true;
            this.lblFileCount.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileCount.Location = new System.Drawing.Point(12, 60);
            this.lblFileCount.Name = "lblFileCount";
            this.lblFileCount.Size = new System.Drawing.Size(46, 12);
            this.lblFileCount.TabIndex = 3;
            this.lblFileCount.Text = "Files: 0";
            // 
            // lblTotalSize
            // 
            this.lblTotalSize.AutoSize = true;
            this.lblTotalSize.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalSize.Location = new System.Drawing.Point(12, 80);
            this.lblTotalSize.Name = "lblTotalSize";
            this.lblTotalSize.Size = new System.Drawing.Size(94, 12);
            this.lblTotalSize.TabIndex = 4;
            this.lblTotalSize.Text = "File sizes: 0 KB";
            // 
            // btnCopy
            // 
            this.btnCopy.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnCopy.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy.Location = new System.Drawing.Point(222, 107);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(80, 25);
            this.btnCopy.TabIndex = 5;
            this.btnCopy.Text = "&Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(308, 107);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 25);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblSeparator
            // 
            this.lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeparator.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSeparator.Location = new System.Drawing.Point(12, 52);
            this.lblSeparator.Name = "lblSeparator";
            this.lblSeparator.Size = new System.Drawing.Size(376, 2);
            this.lblSeparator.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(376, 2);
            this.label1.TabIndex = 7;
            // 
            // FormCopyConfirm
            // 
            this.AcceptButton = this.btnCopy;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(394, 139);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.lblTotalSize);
            this.Controls.Add(this.lblFileCount);
            this.Controls.Add(this.lblSeparator);
            this.Controls.Add(this.lblDestPath);
            this.Controls.Add(this.lblSourcePath);
            this.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCopyConfirm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Copying Files";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblSourcePath;
        private System.Windows.Forms.Label lblDestPath;
        private System.Windows.Forms.Label lblFileCount;
        private System.Windows.Forms.Label lblTotalSize;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnCancel;
        private Label label1;
        private System.Windows.Forms.Label lblSeparator;
    }
} 