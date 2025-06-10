using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalCommander.GUI
{
    public partial class FormSettings : Form
    {
        private Form_TotalCommander _mainForm;

        public FormSettings(Form_TotalCommander mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
        }

        private void InitializeComponent()
        {
            this.groupBoxView = new System.Windows.Forms.GroupBox();
            this.radioSortByName = new System.Windows.Forms.RadioButton();
            this.radioSortBySize = new System.Windows.Forms.RadioButton();
            this.radioSortByDate = new System.Windows.Forms.RadioButton();
            this.radioSortByExtension = new System.Windows.Forms.RadioButton();
            this.groupBoxFunctions = new System.Windows.Forms.GroupBox();
            this.btnFunctionKeys = new System.Windows.Forms.Button();
            this.btnUserExecute = new System.Windows.Forms.Button();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.btnExplorerFont = new System.Windows.Forms.Button();
            this.btnStatusFont = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBoxView.SuspendLayout();
            this.groupBoxFunctions.SuspendLayout();
            this.groupBoxFont.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxView
            // 
            this.groupBoxView.Controls.Add(this.radioSortByExtension);
            this.groupBoxView.Controls.Add(this.radioSortByDate);
            this.groupBoxView.Controls.Add(this.radioSortBySize);
            this.groupBoxView.Controls.Add(this.radioSortByName);
            this.groupBoxView.Location = new System.Drawing.Point(12, 12);
            this.groupBoxView.Name = "groupBoxView";
            this.groupBoxView.Size = new System.Drawing.Size(380, 120);
            this.groupBoxView.TabIndex = 0;
            this.groupBoxView.TabStop = false;
            this.groupBoxView.Text = "View";
            // 
            // radioSortByName
            // 
            this.radioSortByName.AutoSize = true;
            this.radioSortByName.Checked = true;
            this.radioSortByName.Location = new System.Drawing.Point(20, 25);
            this.radioSortByName.Name = "radioSortByName";
            this.radioSortByName.Size = new System.Drawing.Size(105, 19);
            this.radioSortByName.TabIndex = 0;
            this.radioSortByName.TabStop = true;
            this.radioSortByName.Text = "Sort by Name";
            this.radioSortByName.UseVisualStyleBackColor = true;
            // 
            // radioSortBySize
            // 
            this.radioSortBySize.AutoSize = true;
            this.radioSortBySize.Location = new System.Drawing.Point(20, 50);
            this.radioSortBySize.Name = "radioSortBySize";
            this.radioSortBySize.Size = new System.Drawing.Size(93, 19);
            this.radioSortBySize.TabIndex = 1;
            this.radioSortBySize.Text = "Sort by Size";
            this.radioSortBySize.UseVisualStyleBackColor = true;
            // 
            // radioSortByDate
            // 
            this.radioSortByDate.AutoSize = true;
            this.radioSortByDate.Location = new System.Drawing.Point(20, 75);
            this.radioSortByDate.Name = "radioSortByDate";
            this.radioSortByDate.Size = new System.Drawing.Size(97, 19);
            this.radioSortByDate.TabIndex = 2;
            this.radioSortByDate.Text = "Sort by Date";
            this.radioSortByDate.UseVisualStyleBackColor = true;
            // 
            // radioSortByExtension
            // 
            this.radioSortByExtension.AutoSize = true;
            this.radioSortByExtension.Location = new System.Drawing.Point(200, 25);
            this.radioSortByExtension.Name = "radioSortByExtension";
            this.radioSortByExtension.Size = new System.Drawing.Size(126, 19);
            this.radioSortByExtension.TabIndex = 3;
            this.radioSortByExtension.Text = "Sort by Extension";
            this.radioSortByExtension.UseVisualStyleBackColor = true;
            // 
            // groupBoxFunctions
            // 
            this.groupBoxFunctions.Controls.Add(this.btnUserExecute);
            this.groupBoxFunctions.Controls.Add(this.btnFunctionKeys);
            this.groupBoxFunctions.Location = new System.Drawing.Point(12, 138);
            this.groupBoxFunctions.Name = "groupBoxFunctions";
            this.groupBoxFunctions.Size = new System.Drawing.Size(380, 100);
            this.groupBoxFunctions.TabIndex = 1;
            this.groupBoxFunctions.TabStop = false;
            this.groupBoxFunctions.Text = "Functions";
            // 
            // btnFunctionKeys
            // 
            this.btnFunctionKeys.Location = new System.Drawing.Point(20, 30);
            this.btnFunctionKeys.Name = "btnFunctionKeys";
            this.btnFunctionKeys.Size = new System.Drawing.Size(150, 30);
            this.btnFunctionKeys.TabIndex = 0;
            this.btnFunctionKeys.Text = "Function Keys...";
            this.btnFunctionKeys.UseVisualStyleBackColor = true;
            this.btnFunctionKeys.Click += new System.EventHandler(this.btnFunctionKeys_Click);
            // 
            // btnUserExecute
            // 
            this.btnUserExecute.Location = new System.Drawing.Point(200, 30);
            this.btnUserExecute.Name = "btnUserExecute";
            this.btnUserExecute.Size = new System.Drawing.Size(150, 30);
            this.btnUserExecute.TabIndex = 1;
            this.btnUserExecute.Text = "User Execute Options...";
            this.btnUserExecute.UseVisualStyleBackColor = true;
            this.btnUserExecute.Click += new System.EventHandler(this.btnUserExecute_Click);
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Controls.Add(this.btnStatusFont);
            this.groupBoxFont.Controls.Add(this.btnExplorerFont);
            this.groupBoxFont.Location = new System.Drawing.Point(12, 244);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(380, 80);
            this.groupBoxFont.TabIndex = 2;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "Font";
            // 
            // btnExplorerFont
            // 
            this.btnExplorerFont.Location = new System.Drawing.Point(20, 30);
            this.btnExplorerFont.Name = "btnExplorerFont";
            this.btnExplorerFont.Size = new System.Drawing.Size(150, 30);
            this.btnExplorerFont.TabIndex = 0;
            this.btnExplorerFont.Text = "Explorer Font...";
            this.btnExplorerFont.UseVisualStyleBackColor = true;
            this.btnExplorerFont.Click += new System.EventHandler(this.btnExplorerFont_Click);
            // 
            // btnStatusFont
            // 
            this.btnStatusFont.Location = new System.Drawing.Point(200, 30);
            this.btnStatusFont.Name = "btnStatusFont";
            this.btnStatusFont.Size = new System.Drawing.Size(150, 30);
            this.btnStatusFont.TabIndex = 1;
            this.btnStatusFont.Text = "Status Font...";
            this.btnStatusFont.UseVisualStyleBackColor = true;
            this.btnStatusFont.Click += new System.EventHandler(this.btnStatusFont_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(222, 335);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 30);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(312, 335);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FormSettings
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(404, 381);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBoxFont);
            this.Controls.Add(this.groupBoxFunctions);
            this.Controls.Add(this.groupBoxView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.groupBoxView.ResumeLayout(false);
            this.groupBoxView.PerformLayout();
            this.groupBoxFunctions.ResumeLayout(false);
            this.groupBoxFont.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            // Load current settings when the form is displayed
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Set the radio button based on the current sort setting
            int sortMode = Properties.Settings.Default.DefaultSortMode;
            switch (sortMode)
            {
                case 0:
                    radioSortByName.Checked = true;
                    break;
                case 1:
                    radioSortBySize.Checked = true;
                    break;
                case 2:
                    radioSortByDate.Checked = true;
                    break;
                case 3:
                    radioSortByExtension.Checked = true;
                    break;
                default:
                    radioSortByName.Checked = true;
                    break;
            }
        }

        private void btnFunctionKeys_Click(object sender, EventArgs e)
        {
            // Show function keys configuration dialog
            _mainForm.ShowFunctionKeysDialog();
        }

        private void btnUserExecute_Click(object sender, EventArgs e)
        {
            // Show user execute options dialog
            _mainForm.ShowUserExecuteOptionsDialog();
        }

        private void btnExplorerFont_Click(object sender, EventArgs e)
        {
            // Show explorer font settings dialog
            _mainForm.ShowExplorerFontDialog();
        }

        private void btnStatusFont_Click(object sender, EventArgs e)
        {
            // Show status font settings dialog
            _mainForm.ShowStatusFontDialog();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SaveSettings()
        {
            // Save the sort mode setting
            int sortMode = 0;
            if (radioSortByName.Checked) sortMode = 0;
            else if (radioSortBySize.Checked) sortMode = 1;
            else if (radioSortByDate.Checked) sortMode = 2;
            else if (radioSortByExtension.Checked) sortMode = 3;

            Properties.Settings.Default.DefaultSortMode = sortMode;
            Properties.Settings.Default.Save();

            // Apply the sort mode to both browsers
            _mainForm.ApplySortMode(sortMode);
        }

        private System.Windows.Forms.GroupBox groupBoxView;
        private System.Windows.Forms.RadioButton radioSortByName;
        private System.Windows.Forms.RadioButton radioSortBySize;
        private System.Windows.Forms.RadioButton radioSortByDate;
        private System.Windows.Forms.RadioButton radioSortByExtension;
        private System.Windows.Forms.GroupBox groupBoxFunctions;
        private System.Windows.Forms.Button btnFunctionKeys;
        private System.Windows.Forms.Button btnUserExecute;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.Button btnExplorerFont;
        private System.Windows.Forms.Button btnStatusFont;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
} 