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

namespace TotalCommander.GUI
{
    public partial class FormProgressExtract : Form
    {
        private string[] files;
        private int totalFiles;
        private int completedFiles = 0;
        private bool cancelRequested = false;

        // 작업 완료 이벤트 정의
        public event EventHandler OperationCompleted;

        public FormProgressExtract(string[] archiveFiles)
        {
            InitializeComponent();
            files = archiveFiles;
            totalFiles = files.Length;
            
            // Register Load event handler
            this.Load += FormProgressExtract_Load;
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblCurrentFile = new System.Windows.Forms.Label();
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
            this.lblTitle.Text = "압축 파일 풀기";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(16, 42);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(372, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
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
            this.lblStatus.Text = "압축 파일을 처리하는 중...";
            //
            // lblCurrentFile
            //
            this.lblCurrentFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentFile.AutoEllipsis = true;
            this.lblCurrentFile.Location = new System.Drawing.Point(16, 106);
            this.lblCurrentFile.Name = "lblCurrentFile";
            this.lblCurrentFile.Size = new System.Drawing.Size(372, 23);
            this.lblCurrentFile.TabIndex = 3;
            this.lblCurrentFile.Text = "";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(164, 140);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "취소";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormProgressExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 175);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblCurrentFile);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProgressExtract";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "압축 풀기";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblCurrentFile;
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
        /// Set progress (0-100) - 이제 마퀴 스타일이라 진행률은 표시되지 않음
        /// </summary>
        public void SetProgress(int percent)
        {
            // Marquee 스타일에서는 진행률 설정이 필요 없음
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
        
        /// <summary>
        /// 현재 처리 중인 파일 이름 업데이트
        /// </summary>
        public void SetCurrentFile(string fileName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetCurrentFile), fileName);
                return;
            }

            lblCurrentFile.Text = "파일: " + fileName;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            isCancelled = true;
            btnCancel.Enabled = false;
            lblStatus.Text = "취소 중...";
        }

        private void FormProgressExtract_Load(object sender, EventArgs e)
        {
            // 폼이 로드되면 바로 보이도록 설정
            this.Visible = true;
            this.BringToFront();
        }

        /// <summary>
        /// 압축 해제 작업 완료시 호출
        /// </summary>
        public void OnOperationCompleted()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OnOperationCompleted));
                return;
            }

            // 작업 완료 이벤트 발생
            OperationCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 파일 진행률 업데이트
        /// </summary>
        public void UpdateFileProgress(int current, int total)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int>(UpdateFileProgress), current, total);
                return;
            }

            completedFiles = current;
            totalFiles = total;
            
            // Marquee 스타일에서는 진행률 대신 파일 개수 표시
            SetStatus($"압축 해제 중: {current}/{total} 파일 완료");
        }
    }
} 