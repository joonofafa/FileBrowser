namespace TotalCommander.GUI.Settings
{
    partial class KeyPanel
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
            this.groupBoxKeySettings = new System.Windows.Forms.GroupBox();
            this.listViewKeys = new System.Windows.Forms.ListView();
            this.columnKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnOption = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelEdit = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.lblOption = new System.Windows.Forms.Label();
            this.cmbOption = new System.Windows.Forms.ComboBox();
            this.lblAction = new System.Windows.Forms.Label();
            this.cmbAction = new System.Windows.Forms.ComboBox();
            this.lblSelectedKey = new System.Windows.Forms.Label();
            this.txtSelectedKey = new System.Windows.Forms.TextBox();
            this.lblHelp = new System.Windows.Forms.Label();
            this.groupBoxKeySettings.SuspendLayout();
            this.panelEdit.SuspendLayout();
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
            this.lblTitle.Size = new System.Drawing.Size(75, 26);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "기능 키";
            // 
            // groupBoxKeySettings
            // 
            this.groupBoxKeySettings.Controls.Add(this.listViewKeys);
            this.groupBoxKeySettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxKeySettings.Location = new System.Drawing.Point(10, 36);
            this.groupBoxKeySettings.Name = "groupBoxKeySettings";
            this.groupBoxKeySettings.Size = new System.Drawing.Size(1093, 250);
            this.groupBoxKeySettings.TabIndex = 1;
            this.groupBoxKeySettings.TabStop = false;
            this.groupBoxKeySettings.Text = "기능키 설정";
            // 
            // listViewKeys
            // 
            this.listViewKeys.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnKey,
            this.columnAction,
            this.columnOption});
            this.listViewKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewKeys.FullRowSelect = true;
            this.listViewKeys.GridLines = true;
            this.listViewKeys.HideSelection = false;
            this.listViewKeys.Location = new System.Drawing.Point(3, 17);
            this.listViewKeys.MultiSelect = false;
            this.listViewKeys.Name = "listViewKeys";
            this.listViewKeys.Size = new System.Drawing.Size(1087, 230);
            this.listViewKeys.TabIndex = 0;
            this.listViewKeys.UseCompatibleStateImageBehavior = false;
            this.listViewKeys.View = System.Windows.Forms.View.Details;
            this.listViewKeys.SelectedIndexChanged += new System.EventHandler(this.listViewKeys_SelectedIndexChanged);
            // 
            // columnKey
            // 
            this.columnKey.Text = "키";
            this.columnKey.Width = 100;
            // 
            // columnAction
            // 
            this.columnAction.Text = "동작";
            this.columnAction.Width = 200;
            // 
            // columnOption
            // 
            this.columnOption.Text = "옵션";
            this.columnOption.Width = 300;
            // 
            // panelEdit
            // 
            this.panelEdit.Controls.Add(this.btnReset);
            this.panelEdit.Controls.Add(this.btnApply);
            this.panelEdit.Controls.Add(this.lblOption);
            this.panelEdit.Controls.Add(this.cmbOption);
            this.panelEdit.Controls.Add(this.lblAction);
            this.panelEdit.Controls.Add(this.cmbAction);
            this.panelEdit.Controls.Add(this.lblSelectedKey);
            this.panelEdit.Controls.Add(this.txtSelectedKey);
            this.panelEdit.Controls.Add(this.lblHelp);
            this.panelEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelEdit.Location = new System.Drawing.Point(10, 286);
            this.panelEdit.Name = "panelEdit";
            this.panelEdit.Size = new System.Drawing.Size(1093, 200);
            this.panelEdit.TabIndex = 2;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(213, 159);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 30);
            this.btnReset.TabIndex = 8;
            this.btnReset.Text = "초기화(&R)";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(107, 159);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(100, 30);
            this.btnApply.TabIndex = 7;
            this.btnApply.Text = "적용(&A)";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblOption
            // 
            this.lblOption.AutoSize = true;
            this.lblOption.Location = new System.Drawing.Point(20, 125);
            this.lblOption.Name = "lblOption";
            this.lblOption.Size = new System.Drawing.Size(53, 12);
            this.lblOption.TabIndex = 6;
            this.lblOption.Text = "옵션(&O):";
            // 
            // cmbOption
            // 
            this.cmbOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOption.FormattingEnabled = true;
            this.cmbOption.Location = new System.Drawing.Point(107, 122);
            this.cmbOption.Name = "cmbOption";
            this.cmbOption.Size = new System.Drawing.Size(300, 20);
            this.cmbOption.TabIndex = 5;
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.Location = new System.Drawing.Point(20, 95);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(53, 12);
            this.lblAction.TabIndex = 4;
            this.lblAction.Text = "동작(&A):";
            // 
            // cmbAction
            // 
            this.cmbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAction.FormattingEnabled = true;
            this.cmbAction.Location = new System.Drawing.Point(107, 92);
            this.cmbAction.Name = "cmbAction";
            this.cmbAction.Size = new System.Drawing.Size(300, 20);
            this.cmbAction.TabIndex = 3;
            this.cmbAction.SelectedIndexChanged += new System.EventHandler(this.cmbAction_SelectedIndexChanged);
            // 
            // lblSelectedKey
            // 
            this.lblSelectedKey.AutoSize = true;
            this.lblSelectedKey.Location = new System.Drawing.Point(20, 65);
            this.lblSelectedKey.Name = "lblSelectedKey";
            this.lblSelectedKey.Size = new System.Drawing.Size(81, 12);
            this.lblSelectedKey.TabIndex = 2;
            this.lblSelectedKey.Text = "선택한 키(&K):";
            // 
            // txtSelectedKey
            // 
            this.txtSelectedKey.Location = new System.Drawing.Point(107, 62);
            this.txtSelectedKey.Name = "txtSelectedKey";
            this.txtSelectedKey.ReadOnly = true;
            this.txtSelectedKey.Size = new System.Drawing.Size(300, 21);
            this.txtSelectedKey.TabIndex = 1;
            // 
            // lblHelp
            // 
            this.lblHelp.AutoSize = true;
            this.lblHelp.Location = new System.Drawing.Point(20, 20);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(309, 24);
            this.lblHelp.TabIndex = 0;
            this.lblHelp.Text = "위 목록에서 키를 선택하고 동작과 옵션을 지정하세요.\r\n선택한 키에 대한 동작을 변경하려면 적용 버튼을 누르세요.";
            // 
            // KeyPanel
            // 
            this.Controls.Add(this.panelEdit);
            this.Controls.Add(this.groupBoxKeySettings);
            this.Controls.Add(this.lblTitle);
            this.Name = "KeyPanel";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(1113, 1088);
            this.Load += new System.EventHandler(this.KeyPanel_Load);
            this.groupBoxKeySettings.ResumeLayout(false);
            this.panelEdit.ResumeLayout(false);
            this.panelEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox groupBoxKeySettings;
        private System.Windows.Forms.ListView listViewKeys;
        private System.Windows.Forms.ColumnHeader columnKey;
        private System.Windows.Forms.ColumnHeader columnAction;
        private System.Windows.Forms.ColumnHeader columnOption;
        private System.Windows.Forms.Panel panelEdit;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Label lblSelectedKey;
        private System.Windows.Forms.TextBox txtSelectedKey;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.ComboBox cmbAction;
        private System.Windows.Forms.Label lblOption;
        private System.Windows.Forms.ComboBox cmbOption;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnReset;
    }
} 