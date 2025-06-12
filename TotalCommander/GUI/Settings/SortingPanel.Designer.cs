namespace TotalCommander.GUI.Settings
{
    partial class SortingPanel
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
            this.groupBoxSorting = new System.Windows.Forms.GroupBox();
            this.radioSortByName = new System.Windows.Forms.RadioButton();
            this.radioSortByExt = new System.Windows.Forms.RadioButton();
            this.radioSortBySize = new System.Windows.Forms.RadioButton();
            this.radioSortByDate = new System.Windows.Forms.RadioButton();
            this.checkReverse = new System.Windows.Forms.CheckBox();
            this.checkDirsFirst = new System.Windows.Forms.CheckBox();
            this.groupBoxSorting.SuspendLayout();
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
            this.lblTitle.Text = "정렬";
            // 
            // groupBoxSorting
            // 
            this.groupBoxSorting.Controls.Add(this.checkDirsFirst);
            this.groupBoxSorting.Controls.Add(this.checkReverse);
            this.groupBoxSorting.Controls.Add(this.radioSortByDate);
            this.groupBoxSorting.Controls.Add(this.radioSortBySize);
            this.groupBoxSorting.Controls.Add(this.radioSortByExt);
            this.groupBoxSorting.Controls.Add(this.radioSortByName);
            this.groupBoxSorting.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxSorting.Location = new System.Drawing.Point(10, 36);
            this.groupBoxSorting.Name = "groupBoxSorting";
            this.groupBoxSorting.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxSorting.Size = new System.Drawing.Size(480, 150);
            this.groupBoxSorting.TabIndex = 1;
            this.groupBoxSorting.TabStop = false;
            this.groupBoxSorting.Text = "파일 정렬 방법";
            // 
            // radioSortByName
            // 
            this.radioSortByName.AutoSize = true;
            this.radioSortByName.Checked = true;
            this.radioSortByName.Location = new System.Drawing.Point(20, 30);
            this.radioSortByName.Name = "radioSortByName";
            this.radioSortByName.Size = new System.Drawing.Size(71, 16);
            this.radioSortByName.TabIndex = 0;
            this.radioSortByName.TabStop = true;
            this.radioSortByName.Text = "이름순";
            this.radioSortByName.UseVisualStyleBackColor = true;
            // 
            // radioSortByExt
            // 
            this.radioSortByExt.AutoSize = true;
            this.radioSortByExt.Location = new System.Drawing.Point(20, 55);
            this.radioSortByExt.Name = "radioSortByExt";
            this.radioSortByExt.Size = new System.Drawing.Size(83, 16);
            this.radioSortByExt.TabIndex = 1;
            this.radioSortByExt.Text = "확장자순";
            this.radioSortByExt.UseVisualStyleBackColor = true;
            // 
            // radioSortBySize
            // 
            this.radioSortBySize.AutoSize = true;
            this.radioSortBySize.Location = new System.Drawing.Point(20, 80);
            this.radioSortBySize.Name = "radioSortBySize";
            this.radioSortBySize.Size = new System.Drawing.Size(71, 16);
            this.radioSortBySize.TabIndex = 2;
            this.radioSortBySize.Text = "크기순";
            this.radioSortBySize.UseVisualStyleBackColor = true;
            // 
            // radioSortByDate
            // 
            this.radioSortByDate.AutoSize = true;
            this.radioSortByDate.Location = new System.Drawing.Point(20, 105);
            this.radioSortByDate.Name = "radioSortByDate";
            this.radioSortByDate.Size = new System.Drawing.Size(71, 16);
            this.radioSortByDate.TabIndex = 3;
            this.radioSortByDate.Text = "날짜순";
            this.radioSortByDate.UseVisualStyleBackColor = true;
            // 
            // checkReverse
            // 
            this.checkReverse.AutoSize = true;
            this.checkReverse.Location = new System.Drawing.Point(200, 30);
            this.checkReverse.Name = "checkReverse";
            this.checkReverse.Size = new System.Drawing.Size(112, 16);
            this.checkReverse.TabIndex = 4;
            this.checkReverse.Text = "역순으로 정렬";
            this.checkReverse.UseVisualStyleBackColor = true;
            // 
            // checkDirsFirst
            // 
            this.checkDirsFirst.AutoSize = true;
            this.checkDirsFirst.Checked = true;
            this.checkDirsFirst.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDirsFirst.Location = new System.Drawing.Point(200, 55);
            this.checkDirsFirst.Name = "checkDirsFirst";
            this.checkDirsFirst.Size = new System.Drawing.Size(160, 16);
            this.checkDirsFirst.TabIndex = 5;
            this.checkDirsFirst.Text = "폴더를 항상 먼저 표시";
            this.checkDirsFirst.UseVisualStyleBackColor = true;
            // 
            // SortingPanel
            // 
            this.Controls.Add(this.groupBoxSorting);
            this.Controls.Add(this.lblTitle);
            this.Name = "SortingPanel";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(500, 400);
            this.groupBoxSorting.ResumeLayout(false);
            this.groupBoxSorting.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox groupBoxSorting;
        private System.Windows.Forms.RadioButton radioSortByName;
        private System.Windows.Forms.RadioButton radioSortByExt;
        private System.Windows.Forms.RadioButton radioSortBySize;
        private System.Windows.Forms.RadioButton radioSortByDate;
        private System.Windows.Forms.CheckBox checkReverse;
        private System.Windows.Forms.CheckBox checkDirsFirst;
    }
}
