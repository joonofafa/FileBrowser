using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TotalCommander.GUI
{
    public partial class FormManageUserOptions : Form
    {
        private KeySettings keySettings;

        public FormManageUserOptions(KeySettings settings)
        {
            InitializeComponent();
            keySettings = settings;
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lstOptions = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(142, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "사용자 실행 옵션 관리";
            // 
            // lstOptions
            // 
            this.lstOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstOptions.FormattingEnabled = true;
            this.lstOptions.ItemHeight = 12;
            this.lstOptions.Location = new System.Drawing.Point(12, 40);
            this.lstOptions.Name = "lstOptions";
            this.lstOptions.Size = new System.Drawing.Size(350, 232);
            this.lstOptions.TabIndex = 1;
            this.lstOptions.SelectedIndexChanged += new System.EventHandler(this.lstOptions_SelectedIndexChanged);
            this.lstOptions.DoubleClick += new System.EventHandler(this.lstOptions_DoubleClick);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(368, 40);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "추가...";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(368, 69);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 3;
            this.btnEdit.Text = "편집...";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(368, 98);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "삭제";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(368, 249);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "닫기";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // FormManageUserOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 291);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstOptions);
            this.Controls.Add(this.lblTitle);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "FormManageUserOptions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "사용자 실행 옵션 관리";
            this.Load += new System.EventHandler(this.FormManageUserOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListBox lstOptions;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClose;

        private void FormManageUserOptions_Load(object sender, EventArgs e)
        {
            // 옵션 목록 불러오기
            RefreshOptionList();
        }

        private void RefreshOptionList()
        {
            lstOptions.Items.Clear();

            // 사용자 실행 옵션 목록 추가
            foreach (var option in keySettings.UserExecuteOptions)
            {
                lstOptions.Items.Add(option.Name);
            }

            // 버튼 상태 업데이트
            btnEdit.Enabled = btnDelete.Enabled = (lstOptions.SelectedIndex >= 0);
        }

        private void lstOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 선택된 항목에 따라 버튼 상태 업데이트
            btnEdit.Enabled = btnDelete.Enabled = (lstOptions.SelectedIndex >= 0);
        }

        private void lstOptions_DoubleClick(object sender, EventArgs e)
        {
            // 더블 클릭 시 편집 버튼 클릭과 동일한 동작
            if (lstOptions.SelectedIndex >= 0)
            {
                btnEdit_Click(sender, e);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // 새 사용자 실행 옵션 추가
            using (FormUserExecuteOption form = new FormUserExecuteOption(keySettings))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // 설정이 저장되었으므로 목록 새로고침
                    RefreshOptionList();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            // 선택된 사용자 실행 옵션 편집
            if (lstOptions.SelectedIndex >= 0)
            {
                string selectedOptionName = lstOptions.SelectedItem.ToString();
                
                using (FormUserExecuteOption form = new FormUserExecuteOption(keySettings, selectedOptionName))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        // 설정이 저장되었으므로 목록 새로고침
                        RefreshOptionList();
                        
                        // 이전에 선택된 항목 다시 선택
                        int index = lstOptions.Items.IndexOf(selectedOptionName);
                        if (index >= 0)
                        {
                            lstOptions.SelectedIndex = index;
                        }
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 선택된 사용자 실행 옵션 삭제
            if (lstOptions.SelectedIndex >= 0)
            {
                string selectedOptionName = lstOptions.SelectedItem.ToString();
                
                // 삭제 확인
                DialogResult result = MessageBox.Show(
                    $"'{selectedOptionName}' 옵션을 삭제하시겠습니까?\n\n이 옵션을 사용하는 모든 키 설정이 초기화됩니다.",
                    "옵션 삭제 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                
                if (result == DialogResult.Yes)
                {
                    // 옵션 삭제
                    keySettings.RemoveUserExecuteOption(selectedOptionName);
                    keySettings.Save();
                    
                    // 목록 새로고침
                    RefreshOptionList();
                }
            }
        }
    }
} 