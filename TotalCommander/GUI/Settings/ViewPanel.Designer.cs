namespace TotalCommander.GUI.Settings
{
    partial class ViewPanel
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
            this.groupBoxView = new System.Windows.Forms.GroupBox();
            this.checkFullRowSelect = new System.Windows.Forms.CheckBox();
            this.checkShowSystem = new System.Windows.Forms.CheckBox();
            this.checkShowHidden = new System.Windows.Forms.CheckBox();
            this.groupBoxView.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxView
            // 
            this.groupBoxView.Controls.Add(this.checkFullRowSelect);
            this.groupBoxView.Controls.Add(this.checkShowSystem);
            this.groupBoxView.Controls.Add(this.checkShowHidden);
            this.groupBoxView.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxView.Location = new System.Drawing.Point(10, 10);
            this.groupBoxView.Name = "groupBoxView";
            this.groupBoxView.Padding = new System.Windows.Forms.Padding(10);
            this.groupBoxView.Size = new System.Drawing.Size(1093, 150);
            this.groupBoxView.TabIndex = 1;
            this.groupBoxView.TabStop = false;
            this.groupBoxView.Text = "파일 표시 방법";
            // 
            // checkFullRowSelect
            // 
            this.checkFullRowSelect.AutoSize = true;
            this.checkFullRowSelect.Checked = true;
            this.checkFullRowSelect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkFullRowSelect.Location = new System.Drawing.Point(20, 80);
            this.checkFullRowSelect.Name = "checkFullRowSelect";
            this.checkFullRowSelect.Size = new System.Drawing.Size(92, 16);
            this.checkFullRowSelect.TabIndex = 2;
            this.checkFullRowSelect.Text = "전체 행 선택";
            this.checkFullRowSelect.UseVisualStyleBackColor = true;
            // 
            // checkShowSystem
            // 
            this.checkShowSystem.AutoSize = true;
            this.checkShowSystem.Location = new System.Drawing.Point(20, 55);
            this.checkShowSystem.Name = "checkShowSystem";
            this.checkShowSystem.Size = new System.Drawing.Size(116, 16);
            this.checkShowSystem.TabIndex = 1;
            this.checkShowSystem.Text = "시스템 파일 표시";
            this.checkShowSystem.UseVisualStyleBackColor = true;
            // 
            // checkShowHidden
            // 
            this.checkShowHidden.AutoSize = true;
            this.checkShowHidden.Location = new System.Drawing.Point(20, 30);
            this.checkShowHidden.Name = "checkShowHidden";
            this.checkShowHidden.Size = new System.Drawing.Size(104, 16);
            this.checkShowHidden.TabIndex = 0;
            this.checkShowHidden.Text = "숨김 파일 표시";
            this.checkShowHidden.UseVisualStyleBackColor = true;
            // 
            // ViewPanel
            // 
            this.Controls.Add(this.groupBoxView);
            this.Name = "ViewPanel";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(1113, 1088);
            this.groupBoxView.ResumeLayout(false);
            this.groupBoxView.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxView;
        private System.Windows.Forms.CheckBox checkShowHidden;
        private System.Windows.Forms.CheckBox checkShowSystem;
        private System.Windows.Forms.CheckBox checkFullRowSelect;
    }
}
