namespace TotalCommander.GUI.Settings
{
    partial class UserCommandPanel
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
            this.groupBoxCommands = new System.Windows.Forms.GroupBox();
            this.listViewCommands = new System.Windows.Forms.ListView();
            this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnParams = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelEdit = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblParams = new System.Windows.Forms.Label();
            this.txtParams = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblHelp = new System.Windows.Forms.Label();
            this.btnNew = new System.Windows.Forms.Button();
            this.groupBoxCommands.SuspendLayout();
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
            this.lblTitle.Size = new System.Drawing.Size(119, 26);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "사용자 명령";
            // 
            // groupBoxCommands
            // 
            this.groupBoxCommands.Controls.Add(this.listViewCommands);
            this.groupBoxCommands.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxCommands.Location = new System.Drawing.Point(10, 36);
            this.groupBoxCommands.Name = "groupBoxCommands";
            this.groupBoxCommands.Size = new System.Drawing.Size(1093, 250);
            this.groupBoxCommands.TabIndex = 1;
            this.groupBoxCommands.TabStop = false;
            this.groupBoxCommands.Text = "사용자 명령 목록";
            // 
            // listViewCommands
            // 
            this.listViewCommands.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnName,
            this.columnPath,
            this.columnParams});
            this.listViewCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCommands.FullRowSelect = true;
            this.listViewCommands.GridLines = true;
            this.listViewCommands.HideSelection = false;
            this.listViewCommands.Location = new System.Drawing.Point(3, 17);
            this.listViewCommands.MultiSelect = false;
            this.listViewCommands.Name = "listViewCommands";
            this.listViewCommands.Size = new System.Drawing.Size(1087, 230);
            this.listViewCommands.TabIndex = 0;
            this.listViewCommands.UseCompatibleStateImageBehavior = false;
            this.listViewCommands.View = System.Windows.Forms.View.Details;
            this.listViewCommands.SelectedIndexChanged += new System.EventHandler(this.listViewCommands_SelectedIndexChanged);
            // 
            // columnName
            // 
            this.columnName.Text = "이름";
            this.columnName.Width = 150;
            // 
            // columnPath
            // 
            this.columnPath.Text = "실행 파일";
            this.columnPath.Width = 300;
            // 
            // columnParams
            // 
            this.columnParams.Text = "매개변수";
            this.columnParams.Width = 300;
            // 
            // panelEdit
            // 
            this.panelEdit.Controls.Add(this.btnNew);
            this.panelEdit.Controls.Add(this.btnDelete);
            this.panelEdit.Controls.Add(this.btnSave);
            this.panelEdit.Controls.Add(this.btnBrowse);
            this.panelEdit.Controls.Add(this.lblParams);
            this.panelEdit.Controls.Add(this.txtParams);
            this.panelEdit.Controls.Add(this.lblPath);
            this.panelEdit.Controls.Add(this.txtPath);
            this.panelEdit.Controls.Add(this.lblName);
            this.panelEdit.Controls.Add(this.txtName);
            this.panelEdit.Controls.Add(this.lblHelp);
            this.panelEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelEdit.Location = new System.Drawing.Point(10, 286);
            this.panelEdit.Name = "panelEdit";
            this.panelEdit.Size = new System.Drawing.Size(1093, 250);
            this.panelEdit.TabIndex = 2;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(213, 200);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 30);
            this.btnDelete.TabIndex = 9;
            this.btnDelete.Text = "삭제(&D)";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(107, 200);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "저장(&S)";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(513, 90);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(100, 23);
            this.btnBrowse.TabIndex = 7;
            this.btnBrowse.Text = "찾아보기(&B)...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblParams
            // 
            this.lblParams.AutoSize = true;
            this.lblParams.Location = new System.Drawing.Point(20, 126);
            this.lblParams.Name = "lblParams";
            this.lblParams.Size = new System.Drawing.Size(77, 12);
            this.lblParams.TabIndex = 6;
            this.lblParams.Text = "매개변수(&P):";
            // 
            // txtParams
            // 
            this.txtParams.Location = new System.Drawing.Point(107, 123);
            this.txtParams.Multiline = true;
            this.txtParams.Name = "txtParams";
            this.txtParams.Size = new System.Drawing.Size(400, 60);
            this.txtParams.TabIndex = 5;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(20, 95);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(77, 12);
            this.lblPath.TabIndex = 4;
            this.lblPath.Text = "실행 파일(&E):";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(107, 92);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(400, 21);
            this.txtPath.TabIndex = 3;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(20, 65);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(51, 12);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "이름(&N):";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(107, 62);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(400, 21);
            this.txtName.TabIndex = 1;
            // 
            // lblHelp
            // 
            this.lblHelp.AutoSize = true;
            this.lblHelp.Location = new System.Drawing.Point(20, 20);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(469, 24);
            this.lblHelp.TabIndex = 0;
            this.lblHelp.Text = "사용자 정의 명령을 설정합니다. 이 명령들은 기능 키 설정에서 선택하여 사용할 수 있습니다.\r\n변경 내용을 저장하려면 저장 버튼을 누르세요.";
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(20, 200);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(81, 30);
            this.btnNew.TabIndex = 10;
            this.btnNew.Text = "새로 만들기";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // UserCommandPanel
            // 
            this.Controls.Add(this.panelEdit);
            this.Controls.Add(this.groupBoxCommands);
            this.Controls.Add(this.lblTitle);
            this.Name = "UserCommandPanel";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(1113, 1088);
            this.Load += new System.EventHandler(this.UserCommandPanel_Load);
            this.groupBoxCommands.ResumeLayout(false);
            this.panelEdit.ResumeLayout(false);
            this.panelEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox groupBoxCommands;
        private System.Windows.Forms.ListView listViewCommands;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnPath;
        private System.Windows.Forms.ColumnHeader columnParams;
        private System.Windows.Forms.Panel panelEdit;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lblParams;
        private System.Windows.Forms.TextBox txtParams;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnNew;
    }
} 