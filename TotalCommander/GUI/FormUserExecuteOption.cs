using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TotalCommander.GUI
{
    public partial class FormUserExecuteOption : Form
    {
        private KeySettings keySettings;
        private UserExecuteOption currentOption;
        private bool isEditing;

        public FormUserExecuteOption(KeySettings settings)
        {
            InitializeComponent();
            keySettings = settings;
            isEditing = false;
            currentOption = new UserExecuteOption();
        }

        public FormUserExecuteOption(KeySettings settings, string optionName)
        {
            InitializeComponent();
            keySettings = settings;
            isEditing = true;
            
            // 기존 옵션 불러오기
            currentOption = keySettings.GetUserExecuteOptionByName(optionName);
            if (currentOption == null)
            {
                currentOption = new UserExecuteOption();
                isEditing = false;
            }
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblExecutable = new System.Windows.Forms.Label();
            this.txtExecutable = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblParameters = new System.Windows.Forms.Label();
            this.txtParameters = new System.Windows.Forms.TextBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(106, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "사용자 실행 옵션";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(14, 50);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(81, 12);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "실행 옵션 이름:";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(101, 47);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(341, 21);
            this.txtName.TabIndex = 2;
            // 
            // lblExecutable
            // 
            this.lblExecutable.AutoSize = true;
            this.lblExecutable.Location = new System.Drawing.Point(14, 85);
            this.lblExecutable.Name = "lblExecutable";
            this.lblExecutable.Size = new System.Drawing.Size(57, 12);
            this.lblExecutable.TabIndex = 3;
            this.lblExecutable.Text = "실행 옵션:";
            // 
            // txtExecutable
            // 
            this.txtExecutable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExecutable.Location = new System.Drawing.Point(101, 82);
            this.txtExecutable.Name = "txtExecutable";
            this.txtExecutable.Size = new System.Drawing.Size(286, 21);
            this.txtExecutable.TabIndex = 4;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(393, 80);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(49, 23);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "선택";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblParameters
            // 
            this.lblParameters.AutoSize = true;
            this.lblParameters.Location = new System.Drawing.Point(14, 120);
            this.lblParameters.Name = "lblParameters";
            this.lblParameters.Size = new System.Drawing.Size(61, 12);
            this.lblParameters.TabIndex = 6;
            this.lblParameters.Text = "파라미터:";
            // 
            // txtParameters
            // 
            this.txtParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParameters.Location = new System.Drawing.Point(101, 117);
            this.txtParameters.Name = "txtParameters";
            this.txtParameters.Size = new System.Drawing.Size(341, 21);
            this.txtParameters.TabIndex = 7;
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(14, 150);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(428, 49);
            this.lblHint.TabIndex = 8;
            this.lblHint.Text = "힌트: 파라미터에 다음 변수를 사용할 수 있습니다.\r\n{Explorer1:SelectedItem} - 왼쪽 파일 목록에서 선택된 항목 경로\r\n{Explorer2:SelectedItem} - 오른쪽 파일 목록에서 선택된 항목 경로";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(286, 211);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "저장";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(367, 211);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "취소";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormUserExecuteOption
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(454, 246);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.txtParameters);
            this.Controls.Add(this.lblParameters);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtExecutable);
            this.Controls.Add(this.lblExecutable);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUserExecuteOption";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "사용자 실행 옵션";
            this.Load += new System.EventHandler(this.FormUserExecuteOption_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblExecutable;
        private System.Windows.Forms.TextBox txtExecutable;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblParameters;
        private System.Windows.Forms.TextBox txtParameters;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private void FormUserExecuteOption_Load(object sender, EventArgs e)
        {
            // 기존 옵션 값 설정
            if (currentOption != null)
            {
                txtName.Text = currentOption.Name;
                txtExecutable.Text = currentOption.ExecutablePath;
                txtParameters.Text = currentOption.Parameters;
            }

            // 편집 모드인 경우 이름 필드 비활성화
            if (isEditing)
            {
                txtName.ReadOnly = true;
                txtName.BackColor = SystemColors.Control;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "실행 파일 선택";
                dlg.Filter = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*";
                dlg.FilterIndex = 1;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtExecutable.Text = dlg.FileName;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 유효성 검사
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("실행 옵션 이름을 입력하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtExecutable.Text))
            {
                MessageBox.Show("실행 파일 경로를 입력하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtExecutable.Focus();
                return;
            }

            // 이름 중복 검사 (편집 모드가 아닌 경우에만)
            if (!isEditing && keySettings.GetUserExecuteOptionByName(txtName.Text) != null)
            {
                MessageBox.Show("이미 같은 이름의 실행 옵션이 존재합니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }

            // 옵션 업데이트
            currentOption.Name = txtName.Text;
            currentOption.ExecutablePath = txtExecutable.Text;
            currentOption.Parameters = txtParameters.Text;

            // 옵션 저장
            keySettings.AddOrUpdateUserExecuteOption(currentOption);
            keySettings.Save();

            DialogResult = DialogResult.OK;
        }
    }
} 