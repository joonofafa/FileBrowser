using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TotalCommander;

namespace TotalCommander.GUI
{
    public partial class FormStatusFontSettings : Form
    {
        public Font SelectedFont { get; private set; }
        private Font m_InitialFont;

        public FormStatusFontSettings(Font currentFont)
        {
            InitializeComponent();
            m_InitialFont = currentFont;
            SelectedFont = currentFont; // Default is current font
        }

        private void InitializeComponent()
        {
            this.comboBoxFontFamily = new System.Windows.Forms.ComboBox();
            this.comboBoxFontSize = new System.Windows.Forms.ComboBox();
            this.checkBoxBold = new System.Windows.Forms.CheckBox();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.textBoxPreview = new System.Windows.Forms.TextBox();
            this.labelFontFamily = new System.Windows.Forms.Label();
            this.labelFontSize = new System.Windows.Forms.Label();
            this.labelPreview = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxFont = new System.Windows.Forms.GroupBox();
            this.groupBoxStyle = new System.Windows.Forms.GroupBox();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.groupBoxFont.SuspendLayout();
            this.groupBoxStyle.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxFontFamily
            // 
            this.comboBoxFontFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFontFamily.FormattingEnabled = true;
            this.comboBoxFontFamily.Location = new System.Drawing.Point(100, 30);
            this.comboBoxFontFamily.Name = "comboBoxFontFamily";
            this.comboBoxFontFamily.Size = new System.Drawing.Size(200, 23);
            this.comboBoxFontFamily.TabIndex = 0;
            this.comboBoxFontFamily.SelectedIndexChanged += new System.EventHandler(this.comboBoxFontFamily_SelectedIndexChanged);
            // 
            // comboBoxFontSize
            // 
            this.comboBoxFontSize.FormattingEnabled = true;
            this.comboBoxFontSize.Location = new System.Drawing.Point(100, 65);
            this.comboBoxFontSize.Name = "comboBoxFontSize";
            this.comboBoxFontSize.Size = new System.Drawing.Size(80, 23);
            this.comboBoxFontSize.TabIndex = 1;
            this.comboBoxFontSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxFontSize_SelectedIndexChanged);
            this.comboBoxFontSize.TextChanged += new System.EventHandler(this.comboBoxFontSize_TextChanged);
            // 
            // checkBoxBold
            // 
            this.checkBoxBold.AutoSize = true;
            this.checkBoxBold.Location = new System.Drawing.Point(20, 25);
            this.checkBoxBold.Name = "checkBoxBold";
            this.checkBoxBold.Size = new System.Drawing.Size(55, 19);
            this.checkBoxBold.TabIndex = 0;
            this.checkBoxBold.Text = "Bold";
            this.checkBoxBold.UseVisualStyleBackColor = true;
            this.checkBoxBold.CheckedChanged += new System.EventHandler(this.checkBoxBold_CheckedChanged);
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(110, 25);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(54, 19);
            this.checkBoxItalic.TabIndex = 1;
            this.checkBoxItalic.Text = "Italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            this.checkBoxItalic.CheckedChanged += new System.EventHandler(this.checkBoxItalic_CheckedChanged);
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxPreview.Location = new System.Drawing.Point(15, 25);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.Size = new System.Drawing.Size(390, 55);
            this.textBoxPreview.TabIndex = 0;
            this.textBoxPreview.Text = "상태바 글꼴 미리보기";
            this.textBoxPreview.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelFontFamily
            // 
            this.labelFontFamily.AutoSize = true;
            this.labelFontFamily.Location = new System.Drawing.Point(20, 33);
            this.labelFontFamily.Name = "labelFontFamily";
            this.labelFontFamily.Size = new System.Drawing.Size(43, 15);
            this.labelFontFamily.TabIndex = 2;
            this.labelFontFamily.Text = "글꼴:";
            // 
            // labelFontSize
            // 
            this.labelFontSize.AutoSize = true;
            this.labelFontSize.Location = new System.Drawing.Point(20, 68);
            this.labelFontSize.Name = "labelFontSize";
            this.labelFontSize.Size = new System.Drawing.Size(47, 15);
            this.labelFontSize.TabIndex = 3;
            this.labelFontSize.Text = "크기:";
            // 
            // labelPreview
            // 
            this.labelPreview.AutoSize = true;
            this.labelPreview.Location = new System.Drawing.Point(15, 25);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(0, 15);
            this.labelPreview.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(240, 340);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(80, 30);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "확인";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(340, 340);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 30);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "취소";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxFont
            // 
            this.groupBoxFont.Controls.Add(this.labelFontSize);
            this.groupBoxFont.Controls.Add(this.labelFontFamily);
            this.groupBoxFont.Controls.Add(this.comboBoxFontSize);
            this.groupBoxFont.Controls.Add(this.comboBoxFontFamily);
            this.groupBoxFont.Location = new System.Drawing.Point(15, 15);
            this.groupBoxFont.Name = "groupBoxFont";
            this.groupBoxFont.Size = new System.Drawing.Size(320, 110);
            this.groupBoxFont.TabIndex = 0;
            this.groupBoxFont.TabStop = false;
            this.groupBoxFont.Text = "글꼴";
            // 
            // groupBoxStyle
            // 
            this.groupBoxStyle.Controls.Add(this.checkBoxItalic);
            this.groupBoxStyle.Controls.Add(this.checkBoxBold);
            this.groupBoxStyle.Location = new System.Drawing.Point(15, 140);
            this.groupBoxStyle.Name = "groupBoxStyle";
            this.groupBoxStyle.Size = new System.Drawing.Size(320, 60);
            this.groupBoxStyle.TabIndex = 1;
            this.groupBoxStyle.TabStop = false;
            this.groupBoxStyle.Text = "스타일";
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Controls.Add(this.textBoxPreview);
            this.groupBoxPreview.Controls.Add(this.labelPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(15, 215);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(420, 100);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "미리보기";
            // 
            // FormStatusFontSettings
            // 
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(450, 390);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxStyle);
            this.Controls.Add(this.groupBoxFont);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStatusFontSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "상태창 글꼴 설정";
            this.Load += new System.EventHandler(this.FormStatusFontSettings_Load);
            this.groupBoxFont.ResumeLayout(false);
            this.groupBoxFont.PerformLayout();
            this.groupBoxStyle.ResumeLayout(false);
            this.groupBoxStyle.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
            this.ResumeLayout(false);
        }

        private void FormStatusFontSettings_Load(object sender, EventArgs e)
        {
            // Load font families
            foreach (FontFamily fontFamily in FontFamily.Families)
            {
                comboBoxFontFamily.Items.Add(fontFamily.Name);
            }

            // Load common font sizes
            var commonSizes = new object[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22 };
            comboBoxFontSize.Items.AddRange(commonSizes);

            // Apply current font settings
            if (m_InitialFont != null)
            {
                comboBoxFontFamily.SelectedItem = m_InitialFont.FontFamily.Name;
                comboBoxFontSize.SelectedItem = (int)m_InitialFont.Size;
                if (!comboBoxFontSize.Items.Contains((int)m_InitialFont.Size)) {
                    comboBoxFontSize.Items.Add((int)m_InitialFont.Size);
                    comboBoxFontSize.SelectedItem = (int)m_InitialFont.Size;
                }

                checkBoxBold.Checked = m_InitialFont.Bold;
                checkBoxItalic.Checked = m_InitialFont.Italic;
            }
            else
            {
                // Set default values
                comboBoxFontFamily.SelectedIndex = comboBoxFontFamily.Items.IndexOf("Microsoft Sans Serif");
                if (comboBoxFontFamily.SelectedIndex == -1 && comboBoxFontFamily.Items.Count > 0) comboBoxFontFamily.SelectedIndex = 0;
                comboBoxFontSize.SelectedItem = 9;
            }
            
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (comboBoxFontFamily.SelectedItem == null || comboBoxFontSize.SelectedItem == null)
                return;

            try
            {
                string fontFamilyName = comboBoxFontFamily.SelectedItem.ToString();
                float fontSize = Convert.ToSingle(comboBoxFontSize.SelectedItem);
                FontStyle style = FontStyle.Regular;

                if (checkBoxBold.Checked)
                    style |= FontStyle.Bold;
                if (checkBoxItalic.Checked)
                    style |= FontStyle.Italic;

                // Update preview text
                Font previewFont = new Font(fontFamilyName, fontSize, style);
                textBoxPreview.Font = previewFont;
                textBoxPreview.Text = "파일: 123개, 크기: 45.6 MB [상태창 미리보기]";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Font preview error: " + ex.Message);
            }
        }

        private void comboBoxFontFamily_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void comboBoxFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void comboBoxFontSize_TextChanged(object sender, EventArgs e)
        {
            // Handle direct size input
            if (float.TryParse(comboBoxFontSize.Text, out float size))
            {
                 if (size > 0 && size < 72) // Valid size range
                 {
                    UpdatePreview();
                 }
            }
        }

        private void checkBoxBold_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void checkBoxItalic_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxFontFamily.SelectedItem != null && comboBoxFontSize.SelectedItem != null)
            {
                try
                {
                    string fontFamilyName = comboBoxFontFamily.SelectedItem.ToString();
                    float fontSize = Convert.ToSingle(comboBoxFontSize.SelectedItem);
                    FontStyle style = FontStyle.Regular;

                    if (checkBoxBold.Checked)
                        style |= FontStyle.Bold;
                    if (checkBoxItalic.Checked)
                        style |= FontStyle.Italic;

                    SelectedFont = new Font(fontFamilyName, fontSize, style);
                    this.DialogResult = DialogResult.OK;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(
                        this, 
                        "글꼴 설정 오류: " + ex.Message, 
                        "글꼴 오류", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                }
            }
            else
            {
                 MessageBox.Show(
                     this, 
                     "글꼴과 크기를 선택하세요.", 
                     "입력 오류", 
                     MessageBoxButtons.OK, 
                     MessageBoxIcon.Warning);
                 this.DialogResult = DialogResult.None;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private System.Windows.Forms.ComboBox comboBoxFontFamily;
        private System.Windows.Forms.ComboBox comboBoxFontSize;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.TextBox textBoxPreview;
        private System.Windows.Forms.Label labelFontFamily;
        private System.Windows.Forms.Label labelFontSize;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxFont;
        private System.Windows.Forms.GroupBox groupBoxStyle;
        private System.Windows.Forms.GroupBox groupBoxPreview;
    }
} 