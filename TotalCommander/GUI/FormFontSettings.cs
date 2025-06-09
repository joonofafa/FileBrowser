using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TotalCommander;

namespace TotalCommander.GUI
{
    public partial class FormFontSettings : Form
    {
        public Font SelectedFont { get; private set; }
        public bool ApplyToStatusBar { get; private set; }
        private Font m_InitialFont;

        public FormFontSettings(Font currentFont)
        {
            InitializeComponent();
            m_InitialFont = currentFont;
            SelectedFont = currentFont; // Default is current font
            ApplyToStatusBar = true; // Default is to apply to status bar
        }

        private void FormFontSettings_Load(object sender, EventArgs e)
        {
            // Load font families
            foreach (FontFamily fontFamily in FontFamily.Families)
            {
                comboBoxFontFamily.Items.Add(fontFamily.Name);
            }

            // Load common font sizes
            var commonSizes = new object[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            comboBoxFontSize.Items.AddRange(commonSizes);

            // Apply current font settings
            if (m_InitialFont != null)
            {
                comboBoxFontFamily.SelectedItem = m_InitialFont.FontFamily.Name;
                comboBoxFontSize.SelectedItem = (int)m_InitialFont.Size; // Only support integer sizes for now
                if (!comboBoxFontSize.Items.Contains((int)m_InitialFont.Size)) {
                    // If size not in list, add and select it
                    comboBoxFontSize.Items.Add((int)m_InitialFont.Size);
                    comboBoxFontSize.SelectedItem = (int)m_InitialFont.Size;
                }

                checkBoxBold.Checked = m_InitialFont.Bold;
                checkBoxItalic.Checked = m_InitialFont.Italic;
            }
            else
            {
                // Set default values (e.g., system default UI font or specific font)
                comboBoxFontFamily.SelectedIndex = comboBoxFontFamily.Items.IndexOf("Microsoft Sans Serif");
                if (comboBoxFontFamily.SelectedIndex == -1 && comboBoxFontFamily.Items.Count > 0) comboBoxFontFamily.SelectedIndex = 0;
                comboBoxFontSize.SelectedItem = 10;
            }
            
            // Initialize status bar settings
            checkBoxApplyToStatusBar.Checked = true;
            
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

                // Update main preview text
                Font previewFont = new Font(fontFamilyName, fontSize, style);
                textBoxPreview.Font = previewFont;
                textBoxPreview.Text = StringResources.GetString("SampleText"); // Sample text
                
                // Update status bar preview (status bar is typically slightly smaller)
                if (checkBoxApplyToStatusBar.Checked)
                {
                    // Status bar font size is 1pt smaller than main font (optional)
                    float statusFontSize = Math.Max(fontSize - 1, 8); // Minimum size 8pt
                    Font statusFont = new Font(fontFamilyName, statusFontSize, style);
                    labelStatusBarPreview.Font = statusFont;
                }
                else
                {
                    // Set to default system font
                    labelStatusBarPreview.Font = SystemFonts.StatusFont;
                }
            }
            catch (Exception ex)
            {
                // Handle font creation failure (e.g., font doesn't support the style)
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
                 if (size > 0 && size < 200) // Valid size range
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
        
        private void checkBoxApplyToStatusBar_CheckedChanged(object sender, EventArgs e)
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
                    ApplyToStatusBar = checkBoxApplyToStatusBar.Checked;
                    this.DialogResult = DialogResult.OK;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(
                        this, 
                        StringResources.GetString("InvalidFontError", ex.Message), 
                        StringResources.GetString("FontError"), 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None; // Don't close on error
                }
            }
            else
            {
                 MessageBox.Show(
                     this, 
                     StringResources.GetString("SelectFontAndSize"), 
                     StringResources.GetString("InputError"), 
                     MessageBoxButtons.OK, 
                     MessageBoxIcon.Warning);
                 this.DialogResult = DialogResult.None; // Don't close on error
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
} 