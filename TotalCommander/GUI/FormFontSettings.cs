using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TotalCommander.GUI
{
    public partial class FormFontSettings : Form
    {
        public Font SelectedFont { get; private set; }
        private Font m_InitialFont;

        public FormFontSettings(Font currentFont)
        {
            InitializeComponent();
            m_InitialFont = currentFont;
            SelectedFont = currentFont; // 기본값은 현재 폰트
        }

        private void FormFontSettings_Load(object sender, EventArgs e)
        {
            // 글꼴 가족 로드
            foreach (FontFamily fontFamily in FontFamily.Families)
            {
                comboBoxFontFamily.Items.Add(fontFamily.Name);
            }

            // 일반적인 글꼴 크기 로드
            var commonSizes = new object[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            comboBoxFontSize.Items.AddRange(commonSizes);

            // 현재 폰트 설정 반영
            if (m_InitialFont != null)
            {
                comboBoxFontFamily.SelectedItem = m_InitialFont.FontFamily.Name;
                comboBoxFontSize.SelectedItem = (int)m_InitialFont.Size; // 정수 크기만 우선 지원
                if (!comboBoxFontSize.Items.Contains((int)m_InitialFont.Size)) {
                    // 목록에 없는 크기면 직접 추가하고 선택
                    comboBoxFontSize.Items.Add((int)m_InitialFont.Size);
                    comboBoxFontSize.SelectedItem = (int)m_InitialFont.Size;
                }

                checkBoxBold.Checked = m_InitialFont.Bold;
                checkBoxItalic.Checked = m_InitialFont.Italic;
            }
            else
            {
                // 기본값 설정 (예: 시스템 기본 UI 폰트 또는 특정 폰트)
                comboBoxFontFamily.SelectedIndex = comboBoxFontFamily.Items.IndexOf("Microsoft Sans Serif");
                if (comboBoxFontFamily.SelectedIndex == -1 && comboBoxFontFamily.Items.Count > 0) comboBoxFontFamily.SelectedIndex = 0;
                comboBoxFontSize.SelectedItem = 10;
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

                textBoxPreview.Font = new Font(fontFamilyName, fontSize, style);
                textBoxPreview.Text = "AaBbYyZz 가나다라 123"; // 샘플 텍스트
            }
            catch (Exception ex)
            {
                // 글꼴 생성 실패 시 (예: 해당 스타일을 지원하지 않는 글꼴)
                // textBoxPreview.Text = "미리보기 오류";
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
            // 사용자가 직접 크기를 입력하는 경우도 처리
            if (float.TryParse(comboBoxFontSize.Text, out float size))
            {
                 if (size > 0 && size < 200) // 유효한 크기 범위
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
                    MessageBox.Show(this, "선택한 글꼴을 적용할 수 없습니다: " + ex.Message, "글꼴 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None; // 오류 시 닫히지 않도록
                }
            }
            else
            {
                 MessageBox.Show(this, "글꼴과 크기를 선택해주세요.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 this.DialogResult = DialogResult.None; // 오류 시 닫히지 않도록
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
} 