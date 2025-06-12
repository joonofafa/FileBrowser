namespace TotalCommander.GUI.Settings
{
    partial class FontPanel
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.btnUseDefault = new System.Windows.Forms.Button();
            this.lblTargetName = new System.Windows.Forms.Label();
            this.cmbTarget = new System.Windows.Forms.ComboBox();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.chkUnderline = new System.Windows.Forms.CheckBox();
            this.chkStrikeout = new System.Windows.Forms.CheckBox();
            this.chkItalic = new System.Windows.Forms.CheckBox();
            this.chkBold = new System.Windows.Forms.CheckBox();
            this.numFontSize = new System.Windows.Forms.NumericUpDown();
            this.lblFontSize = new System.Windows.Forms.Label();
            this.lblFontName = new System.Windows.Forms.Label();
            this.cmbFont = new System.Windows.Forms.ComboBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.groupBoxFont.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.Location = new System.Drawing.Point(10, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblTitle.Size = new System.Drawing.Size(41, 26);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "글꼴";
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Controls.Add(this.btnUseDefault);
            this.groupBoxFont.Controls.Add(this.lblTargetName);
            this.groupBoxFont.Controls.Add(this.cmbTarget);
            this.groupBoxFont.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxFont.Location = new System.Drawing.Point(10, 36);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(1093, 60);
            this.groupBoxFont.TabIndex = 1;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "설정 항목";
            // 
            // btnUseDefault
            // 
            this.btnUseDefault.Location = new System.Drawing.Point(392, 21);
            this.btnUseDefault.Name = "btnUseDefault";
            this.btnUseDefault.Size = new System.Drawing.Size(100, 23);
            this.btnUseDefault.TabIndex = 2;
            this.btnUseDefault.Text = "기본값 사용(&U)";
            this.btnUseDefault.UseVisualStyleBackColor = true;
            this.btnUseDefault.Click += new System.EventHandler(this.btnUseDefault_Click);
            // 
            // lblTargetName
            // 
            this.lblTargetName.AutoSize = true;
            this.lblTargetName.Location = new System.Drawing.Point(20, 24);
            this.lblTargetName.Name = "lblTargetName";
            this.lblTargetName.Size = new System.Drawing.Size(79, 12);
            this.lblTargetName.TabIndex = 1;
            this.lblTargetName.Text = "설정 표시(&T):";
            // 
            // cmbTarget
            // 
            this.cmbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTarget.FormattingEnabled = true;
            this.cmbTarget.Items.AddRange(new object[] {
            "폴더창",
            "파일뷰어창",
            "상태표시줄",
            "주소표시줄"});
            this.cmbTarget.Location = new System.Drawing.Point(107, 21);
            this.cmbTarget.Name = "cmbTarget";
            this.cmbTarget.Size = new System.Drawing.Size(260, 20);
            this.cmbTarget.TabIndex = 0;
            this.cmbTarget.SelectedIndexChanged += new System.EventHandler(this.cmbTarget_SelectedIndexChanged);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Controls.Add(this.chkUnderline);
            this.groupBoxPreview.Controls.Add(this.chkStrikeout);
            this.groupBoxPreview.Controls.Add(this.chkItalic);
            this.groupBoxPreview.Controls.Add(this.chkBold);
            this.groupBoxPreview.Controls.Add(this.numFontSize);
            this.groupBoxPreview.Controls.Add(this.lblFontSize);
            this.groupBoxPreview.Controls.Add(this.lblFontName);
            this.groupBoxPreview.Controls.Add(this.cmbFont);
            this.groupBoxPreview.Controls.Add(this.lblPreview);
            this.groupBoxPreview.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxPreview.Location = new System.Drawing.Point(10, 96);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(1093, 200);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            // 
            // chkUnderline
            // 
            this.chkUnderline.AutoSize = true;
            this.chkUnderline.Location = new System.Drawing.Point(252, 75);
            this.chkUnderline.Name = "chkUnderline";
            this.chkUnderline.Size = new System.Drawing.Size(65, 16);
            this.chkUnderline.TabIndex = 8;
            this.chkUnderline.Text = "밑줄(&L)";
            this.chkUnderline.UseVisualStyleBackColor = true;
            this.chkUnderline.CheckedChanged += new System.EventHandler(this.FontStyleChanged);
            // 
            // chkStrikeout
            // 
            this.chkStrikeout.AutoSize = true;
            this.chkStrikeout.Location = new System.Drawing.Point(327, 75);
            this.chkStrikeout.Name = "chkStrikeout";
            this.chkStrikeout.Size = new System.Drawing.Size(78, 16);
            this.chkStrikeout.TabIndex = 7;
            this.chkStrikeout.Text = "취소선(&K)";
            this.chkStrikeout.UseVisualStyleBackColor = true;
            this.chkStrikeout.CheckedChanged += new System.EventHandler(this.FontStyleChanged);
            // 
            // chkItalic
            // 
            this.chkItalic.AutoSize = true;
            this.chkItalic.Location = new System.Drawing.Point(173, 75);
            this.chkItalic.Name = "chkItalic";
            this.chkItalic.Size = new System.Drawing.Size(73, 16);
            this.chkItalic.TabIndex = 6;
            this.chkItalic.Text = "기울임(&I)";
            this.chkItalic.UseVisualStyleBackColor = true;
            this.chkItalic.CheckedChanged += new System.EventHandler(this.FontStyleChanged);
            // 
            // chkBold
            // 
            this.chkBold.AutoSize = true;
            this.chkBold.Location = new System.Drawing.Point(107, 75);
            this.chkBold.Name = "chkBold";
            this.chkBold.Size = new System.Drawing.Size(66, 16);
            this.chkBold.TabIndex = 5;
            this.chkBold.Text = "굵게(&B)";
            this.chkBold.UseVisualStyleBackColor = true;
            this.chkBold.CheckedChanged += new System.EventHandler(this.FontStyleChanged);
            // 
            // numFontSize
            // 
            this.numFontSize.Location = new System.Drawing.Point(370, 40);
            this.numFontSize.Maximum = new decimal(new int[] {
            72,
            0,
            0,
            0});
            this.numFontSize.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numFontSize.Name = "numFontSize";
            this.numFontSize.Size = new System.Drawing.Size(50, 21);
            this.numFontSize.TabIndex = 4;
            this.numFontSize.Value = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.numFontSize.ValueChanged += new System.EventHandler(this.numFontSize_ValueChanged);
            // 
            // lblFontSize
            // 
            this.lblFontSize.AutoSize = true;
            this.lblFontSize.Location = new System.Drawing.Point(320, 42);
            this.lblFontSize.Name = "lblFontSize";
            this.lblFontSize.Size = new System.Drawing.Size(51, 12);
            this.lblFontSize.TabIndex = 3;
            this.lblFontSize.Text = "크기(&S):";
            // 
            // lblFontName
            // 
            this.lblFontName.AutoSize = true;
            this.lblFontName.Location = new System.Drawing.Point(20, 42);
            this.lblFontName.Name = "lblFontName";
            this.lblFontName.Size = new System.Drawing.Size(84, 12);
            this.lblFontName.TabIndex = 2;
            this.lblFontName.Text = "글꼴(폰트)(&F):";
            // 
            // cmbFont
            // 
            this.cmbFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFont.FormattingEnabled = true;
            this.cmbFont.Location = new System.Drawing.Point(107, 39);
            this.cmbFont.Name = "cmbFont";
            this.cmbFont.Size = new System.Drawing.Size(200, 20);
            this.cmbFont.TabIndex = 1;
            this.cmbFont.SelectedIndexChanged += new System.EventHandler(this.cmbFont_SelectedIndexChanged);
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Font = new System.Drawing.Font("D2Coding", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblPreview.Location = new System.Drawing.Point(105, 120);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(120, 18);
            this.lblPreview.TabIndex = 0;
            this.lblPreview.Text = "가나다라1234!@#$";
            // 
            // FontPanel
            // 
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxFont);
            this.Controls.Add(this.lblTitle);
            this.Name = "FontPanel";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(1113, 1088);
            this.Load += new System.EventHandler(this.FontPanel_Load);
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.ComboBox cmbFont;
        private System.Windows.Forms.Label lblFontName;
        private System.Windows.Forms.Label lblFontSize;
        private System.Windows.Forms.NumericUpDown numFontSize;
        private System.Windows.Forms.CheckBox chkBold;
        private System.Windows.Forms.CheckBox chkItalic;
        private System.Windows.Forms.CheckBox chkStrikeout;
        private System.Windows.Forms.CheckBox chkUnderline;
        private System.Windows.Forms.ComboBox cmbTarget;
        private System.Windows.Forms.Label lblTargetName;
        private System.Windows.Forms.Button btnUseDefault;
    }
}
