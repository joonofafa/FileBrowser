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
        
        // Available parameter variables list
        private readonly Dictionary<string, string> parameterVariables = new Dictionary<string, string>
        {
            { "{SelectedItemFullPath:LeftExplorer}", StringResources.GetString("LeftSelectedItemPath") },
            { "{SelectedItemDirPath:LeftExplorer}", StringResources.GetString("LeftSelectedItemDirPath") },
            { "{SelectedItemFullPath:RightExplorer}", StringResources.GetString("RightSelectedItemPath") },
            { "{SelectedItemDirPath:RightExplorer}", StringResources.GetString("RightSelectedItemDirPath") },
            { "{SelectedItemFullPath:FocusingExplorer}", StringResources.GetString("FocusingSelectedItemPath") },
            { "{SelectedItemDirPath:FocusingExplorer}", StringResources.GetString("FocusingSelectedItemDirPath") }
        };

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
            
            // Load existing option
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
            this.pnlVariables = new System.Windows.Forms.Panel();
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
            this.lblTitle.Text = StringResources.GetString("UserExecuteOptionTitle");
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(14, 50);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(81, 12);
            this.lblName.TabIndex = 1;
            this.lblName.Text = StringResources.GetString("ExecuteOptionName");
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
            this.lblExecutable.Text = StringResources.GetString("ExecutableOption");
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
            this.btnBrowse.Text = StringResources.GetString("Browse");
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
            this.lblParameters.Text = StringResources.GetString("Parameters");
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
            this.lblHint.Size = new System.Drawing.Size(428, 20);
            this.lblHint.TabIndex = 8;
            this.lblHint.Text = StringResources.GetString("ParametersHint");
            // 
            // pnlVariables
            // 
            this.pnlVariables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlVariables.BorderStyle = BorderStyle.FixedSingle;
            this.pnlVariables.Location = new System.Drawing.Point(16, 173);
            this.pnlVariables.Name = "pnlVariables";
            this.pnlVariables.Size = new System.Drawing.Size(426, 100);
            this.pnlVariables.TabIndex = 9;
            this.pnlVariables.AutoScroll = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(286, 286);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = StringResources.GetString("Save");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(367, 286);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = StringResources.GetString("Cancel");
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormUserExecuteOption
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(454, 321);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.pnlVariables);
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
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = StringResources.GetString("UserExecuteOptionTitle");
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
        private System.Windows.Forms.Panel pnlVariables;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        private void FormUserExecuteOption_Load(object sender, EventArgs e)
        {
            // Load current option data if editing
            if (currentOption != null)
            {
                txtName.Text = currentOption.Name;
                txtExecutable.Text = currentOption.ExecutablePath;
                txtParameters.Text = currentOption.Parameters;
                
                // Disable name field if editing
                if (isEditing)
                {
                    txtName.Enabled = false;
                }
            }
            
            InitializeParameterVariables();
        }
        
        private void InitializeParameterVariables()
        {
            // Initialize parameter variables panel
            int y = 5;
            foreach (var variable in parameterVariables)
            {
                LinkLabel link = new LinkLabel();
                link.Text = variable.Key;
                link.Tag = variable.Key;
                link.Location = new Point(5, y);
                link.AutoSize = true;
                link.LinkClicked += LinkLabel_LinkClicked;
                
                Label description = new Label();
                description.Text = variable.Value;
                description.Location = new Point(250, y);
                description.AutoSize = true;
                
                pnlVariables.Controls.Add(link);
                pnlVariables.Controls.Add(description);
                
                y += 20;
            }
        }
        
        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Add the variable to the parameters textbox
            LinkLabel link = sender as LinkLabel;
            if (link != null && link.Tag is string)
            {
                txtParameters.Text += link.Tag.ToString() + " ";
                txtParameters.Focus();
                txtParameters.SelectionStart = txtParameters.Text.Length;
            }
        }
        
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Browse for executable
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = StringResources.GetString("SelectExecutableTitle");
            dlg.Filter = StringResources.GetString("ExecutableFilter");
            
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtExecutable.Text = dlg.FileName;
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(StringResources.GetString("ExecuteOptionNameRequired"), StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtExecutable.Text))
            {
                MessageBox.Show(StringResources.GetString("ExecutablePathRequired"), StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtExecutable.Focus();
                return;
            }
            
            // Check if name already exists when adding new option
            if (!isEditing && keySettings.GetUserExecuteOptionByName(txtName.Text) != null)
            {
                MessageBox.Show(StringResources.GetString("ExecuteOptionNameExists"), StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtName.Focus();
                return;
            }
            
            // Save option
            currentOption.Name = txtName.Text;
            currentOption.ExecutablePath = txtExecutable.Text;
            currentOption.Parameters = txtParameters.Text;
            
            keySettings.AddOrUpdateUserExecuteOption(currentOption);
            
            DialogResult = DialogResult.OK;
            Close();
        }
    }
} 