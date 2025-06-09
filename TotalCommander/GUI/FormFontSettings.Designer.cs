using TotalCommander;

namespace TotalCommander.GUI
{
    partial class FormFontSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelFontFamily = new System.Windows.Forms.Label();
            this.comboBoxFontFamily = new System.Windows.Forms.ComboBox();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.comboBoxFontSize = new System.Windows.Forms.ComboBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.textBoxPreview = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxStatusBar = new System.Windows.Forms.GroupBox();
            this.checkBoxApplyToStatusBar = new System.Windows.Forms.CheckBox();
            this.labelStatusBarPreview = new System.Windows.Forms.Label();
            this.groupBoxPreview.SuspendLayout();
            this.groupBoxStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelFontFamily
            // 
            this.labelFontFamily.AutoSize = true;
            this.labelFontFamily.Location = new System.Drawing.Point(12, 15);
            this.labelFontFamily.Name = "labelFontFamily";
            this.labelFontFamily.Size = new System.Drawing.Size(54, 12);
            this.labelFontFamily.TabIndex = 0;
            this.labelFontFamily.Text = StringResources.GetString("FontFamily");
            // 
            // comboBoxFontFamily
            // 
            this.comboBoxFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontFamily.FormattingEnabled = true;
            this.comboBoxFontFamily.Location = new System.Drawing.Point(72, 12);
            this.comboBoxFontFamily.Name = "comboBoxFontFamily";
            this.comboBoxFontFamily.Size = new System.Drawing.Size(200, 20);
            this.comboBoxFontFamily.TabIndex = 1;
            this.comboBoxFontFamily.SelectedIndexChanged += new System.EventHandler(this.comboBoxFontFamily_SelectedIndexChanged);
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(12, 41);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(54, 12);
            this.labelFontSize.TabIndex = 2;
            this.labelFontSize.Text = StringResources.GetString("FontSize");
            // 
            // comboBoxFontSize
            // 
            this.comboBoxFontSize.FormattingEnabled = true; // ?�용?��? 직접 ?�력 가?�하?�록 DropDownList?�서 변�?
            this.comboBoxFontSize.Location = new System.Drawing.Point(72, 38);
            this.comboBoxFontSize.Name = "comboBoxFontSize";
            this.comboBoxFontSize.Size = new System.Drawing.Size(75, 20);
            this.comboBoxFontSize.TabIndex = 3;
            this.comboBoxFontSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxFontSize_SelectedIndexChanged);
            this.comboBoxFontSize.TextChanged += new System.EventHandler(this.comboBoxFontSize_TextChanged);
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Location = new System.Drawing.Point(160, 40);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(62, 16);
            this.checkBoxBold.TabIndex = 4;
            this.checkBoxBold.Text = StringResources.GetString("Bold");
            this.checkBoxBold.UseVisualStyleBackColor = true;
            this.checkBoxBold.CheckedChanged += new System.EventHandler(this.checkBoxBold_CheckedChanged);
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(228, 40);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(86, 16);
            this.checkBoxItalic.TabIndex = 5;
            this.checkBoxItalic.Text = StringResources.GetString("Italic");
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            this.checkBoxItalic.CheckedChanged += new System.EventHandler(this.checkBoxItalic_CheckedChanged);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Controls.Add(this.textBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 70);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(300, 100);
            this.groupBoxPreview.TabIndex = 6;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = StringResources.GetString("Preview");
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPreview.Location = new System.Drawing.Point(3, 17);
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.Size = new System.Drawing.Size(294, 80);
            this.textBoxPreview.TabIndex = 0;
            this.textBoxPreview.Text = StringResources.GetString("SampleText");
            this.textBoxPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxStatusBar
            // 
            this.groupBoxStatusBar.Controls.Add(this.labelStatusBarPreview);
            this.groupBoxStatusBar.Controls.Add(this.checkBoxApplyToStatusBar);
            this.groupBoxStatusBar.Location = new System.Drawing.Point(12, 180);
            this.groupBoxStatusBar.Name = "groupBoxStatusBar";
            this.groupBoxStatusBar.Size = new System.Drawing.Size(300, 70);
            this.groupBoxStatusBar.TabIndex = 7;
            this.groupBoxStatusBar.TabStop = false;
            this.groupBoxStatusBar.Text = StringResources.GetString("StatusBarPreview");
            // 
            // checkBoxApplyToStatusBar
            // 
            this.checkBoxApplyToStatusBar.AutoSize = true;
            this.checkBoxApplyToStatusBar.Checked = true;
            this.checkBoxApplyToStatusBar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxApplyToStatusBar.Location = new System.Drawing.Point(10, 20);
            this.checkBoxApplyToStatusBar.Name = "checkBoxApplyToStatusBar";
            this.checkBoxApplyToStatusBar.Size = new System.Drawing.Size(194, 16);
            this.checkBoxApplyToStatusBar.TabIndex = 0;
            this.checkBoxApplyToStatusBar.Text = StringResources.GetString("ApplyToStatusBar");
            this.checkBoxApplyToStatusBar.UseVisualStyleBackColor = true;
            // 
            // labelStatusBarPreview
            // 
            this.labelStatusBarPreview.AutoSize = true;
            this.labelStatusBarPreview.Location = new System.Drawing.Point(10, 45);
            this.labelStatusBarPreview.Name = "labelStatusBarPreview";
            this.labelStatusBarPreview.Size = new System.Drawing.Size(282, 12);
            this.labelStatusBarPreview.TabIndex = 1;
            this.labelStatusBarPreview.Text = "0 files, 0 bytes (Total 0 bytes) | Dir 5, Files 10";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(156, 260);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = StringResources.GetString("OK");
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(237, 260);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = StringResources.GetString("Cancel");
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormFontSettings
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(324, 295);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxStatusBar);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.checkBoxItalic);
            this.Controls.Add(this.checkBoxBold);
            this.Controls.Add(this.comboBoxFontSize);
            this.Controls.Add(this.labelFontSize);
            this.Controls.Add(this.comboBoxFontFamily);
            this.Controls.Add(this.labelFontFamily);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFontSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = StringResources.GetString("FontSettingsTitle");
            this.Load += new System.EventHandler(this.FormFontSettings_Load);
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxStatusBar.ResumeLayout(false);
            this.groupBoxStatusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelFontFamily;
        private System.Windows.Forms.ComboBox comboBoxFontFamily;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.ComboBox comboBoxFontSize;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Label textBoxPreview; // Label�?변�?
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxStatusBar;
        private System.Windows.Forms.Label labelStatusBarPreview;
        private System.Windows.Forms.CheckBox checkBoxApplyToStatusBar;
    }
} 
