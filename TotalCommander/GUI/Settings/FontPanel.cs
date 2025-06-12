using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Text;
using System.Collections.Generic;

namespace TotalCommander.GUI.Settings
{
    public partial class FontPanel : SettingsPanelBase
    {
        private Font _folderFont;
        private Font _viewerFont;
        private Font _statusFont;
        private Font _addressBarFont;
        private Font _currentFont;
        private string _previewText = "가나다라1234!@#$";
        private List<string> _installedFonts;
        private int _currentTargetIndex = 0;

        public FontPanel()
        {
            InitializeComponent();
            SetPanelName("글꼴");
        }

        private void FontPanel_Load(object sender, EventArgs e)
        {
            // 시스템에 설치된 폰트 목록 가져오기
            LoadInstalledFonts();
            
            // 콤보박스에 폰트 목록 채우기
            cmbFont.Items.Clear();
            foreach (string fontName in _installedFonts)
            {
                cmbFont.Items.Add(fontName);
            }
            
            // 기본 폰트 선택
            if (cmbFont.Items.Count > 0)
            {
                cmbFont.SelectedIndex = cmbFont.Items.IndexOf("굴림");
                if (cmbFont.SelectedIndex < 0)
                    cmbFont.SelectedIndex = 0;
            }
            
            // 설정 대상 콤보박스 초기화
            if (cmbTarget.Items.Count > 0)
                cmbTarget.SelectedIndex = 0;
            
            // 기본 폰트 설정
            UpdateUIFromSettings();
        }

        /// <summary>
        /// 시스템에 설치된 폰트 목록 가져오기
        /// </summary>
        private void LoadInstalledFonts()
        {
            _installedFonts = new List<string>();
            
            using (InstalledFontCollection fontsCollection = new InstalledFontCollection())
            {
                FontFamily[] fontFamilies = fontsCollection.Families;
                foreach (FontFamily family in fontFamilies)
                {
                    _installedFonts.Add(family.Name);
                }
            }
            
            // 알파벳순 정렬
            _installedFonts.Sort();
        }

        /// <summary>
        /// 현재 설정 로드
        /// </summary>
        public override void LoadSettings()
        {
            try
            {
                // 설정에서 글꼴 로드
                _folderFont = GetFontFromSettings("FolderFont", "굴림", 9);
                _viewerFont = GetFontFromSettings("ViewerFont", "굴림체", 10);
                _statusFont = GetFontFromSettings("StatusFont", "굴림", 9);
                _addressBarFont = GetFontFromSettings("AddressBarFont", "굴림", 9);
                
                // UI 업데이트
                UpdateUIFromSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("글꼴 설정 로드 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 기본 글꼴로 설정
                _folderFont = new Font("굴림", 9);
                _viewerFont = new Font("굴림체", 10);
                _statusFont = new Font("굴림", 9);
                _addressBarFont = new Font("굴림", 9);
                UpdateUIFromSettings();
            }
        }

        /// <summary>
        /// 설정에서 글꼴 가져오기
        /// </summary>
        private Font GetFontFromSettings(string settingName, string defaultFontName, float defaultSize)
        {
            string fontString = "";
            
            switch (settingName)
            {
                case "FolderFont":
                    fontString = Properties.Settings.Default.FolderFont;
                    break;
                case "ViewerFont":
                    fontString = Properties.Settings.Default.ViewerFont;
                    break;
                case "StatusFont":
                    fontString = Properties.Settings.Default.StatusFont;
                    break;
                case "AddressBarFont":
                    fontString = Properties.Settings.Default.AddressBarFont;
                    break;
                case "ExplorerFont": // 이전 버전과의 호환성
                    fontString = Properties.Settings.Default.ExplorerFont;
                    break;
            }
            
            if (!string.IsNullOrEmpty(fontString))
                return FontConverter.FromString(fontString);
            else
                return new Font(defaultFontName, defaultSize);
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public override void SaveSettings()
        {
            // 현재 UI 상태를 해당 폰트에 저장
            UpdateCurrentFontFromUI();
            
            // 글꼴 설정 저장
            if (_folderFont != null)
                Properties.Settings.Default.FolderFont = FontConverter.ToString(_folderFont);
            
            if (_viewerFont != null)
                Properties.Settings.Default.ViewerFont = FontConverter.ToString(_viewerFont);
            
            if (_statusFont != null)
                Properties.Settings.Default.StatusFont = FontConverter.ToString(_statusFont);
            
            if (_addressBarFont != null)
                Properties.Settings.Default.AddressBarFont = FontConverter.ToString(_addressBarFont);
        }

        /// <summary>
        /// 현재 UI 상태를 해당 폰트에 저장
        /// </summary>
        private void UpdateCurrentFontFromUI()
        {
            if (cmbFont.SelectedItem == null) return;
            
            try
            {
                string fontName = cmbFont.SelectedItem.ToString();
                float fontSize = (float)numFontSize.Value;
                
                FontStyle style = FontStyle.Regular;
                if (chkBold.Checked) style |= FontStyle.Bold;
                if (chkItalic.Checked) style |= FontStyle.Italic;
                if (chkUnderline.Checked) style |= FontStyle.Underline;
                if (chkStrikeout.Checked) style |= FontStyle.Strikeout;
                
                Font newFont = new Font(fontName, fontSize, style);
                
                // 현재 선택된 대상에 따라 폰트 설정
                switch (_currentTargetIndex)
                {
                    case 0: // 폴더창
                        _folderFont = newFont;
                        break;
                    case 1: // 파일뷰어창
                        _viewerFont = newFont;
                        break;
                    case 2: // 상태표시줄
                        _statusFont = newFont;
                        break;
                    case 3: // 주소표시줄
                        _addressBarFont = newFont;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("폰트 설정 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 현재 선택된 대상의 폰트 설정으로 UI 업데이트
        /// </summary>
        private void UpdateUIFromSettings()
        {
            switch (_currentTargetIndex)
            {
                case 0: // 폴더창
                    _currentFont = _folderFont;
                    break;
                case 1: // 파일뷰어창
                    _currentFont = _viewerFont;
                    break;
                case 2: // 상태표시줄
                    _currentFont = _statusFont;
                    break;
                case 3: // 주소표시줄
                    _currentFont = _addressBarFont;
                    break;
            }
            
            if (_currentFont != null)
            {
                // 폰트 이름
                int fontIndex = cmbFont.Items.IndexOf(_currentFont.FontFamily.Name);
                if (fontIndex >= 0)
                    cmbFont.SelectedIndex = fontIndex;
                
                // 폰트 크기
                numFontSize.Value = (decimal)_currentFont.Size;
                
                // 폰트 스타일
                chkBold.Checked = _currentFont.Bold;
                chkItalic.Checked = _currentFont.Italic;
                chkUnderline.Checked = _currentFont.Underline;
                chkStrikeout.Checked = _currentFont.Strikeout;
                
                // 미리보기 업데이트
                UpdatePreview();
            }
        }

        /// <summary>
        /// 미리보기 업데이트
        /// </summary>
        private void UpdatePreview()
        {
            if (_currentFont == null) return;
            
            lblPreview.Font = _currentFont;
            lblPreview.Text = _previewText;
        }

        /// <summary>
        /// 폰트 변경 이벤트 핸들러
        /// </summary>
        private void cmbFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreviewFromUI();
        }

        /// <summary>
        /// 폰트 크기 변경 이벤트 핸들러
        /// </summary>
        private void numFontSize_ValueChanged(object sender, EventArgs e)
        {
            UpdatePreviewFromUI();
        }

        /// <summary>
        /// 폰트 스타일 변경 이벤트 핸들러
        /// </summary>
        private void FontStyleChanged(object sender, EventArgs e)
        {
            UpdatePreviewFromUI();
        }

        /// <summary>
        /// UI에서 현재 선택된 폰트 설정으로 미리보기 업데이트
        /// </summary>
        private void UpdatePreviewFromUI()
        {
            if (cmbFont.SelectedItem == null) return;
            
            try
            {
                string fontName = cmbFont.SelectedItem.ToString();
                float fontSize = (float)numFontSize.Value;
                
                FontStyle style = FontStyle.Regular;
                if (chkBold.Checked) style |= FontStyle.Bold;
                if (chkItalic.Checked) style |= FontStyle.Italic;
                if (chkUnderline.Checked) style |= FontStyle.Underline;
                if (chkStrikeout.Checked) style |= FontStyle.Strikeout;
                
                Font previewFont = new Font(fontName, fontSize, style);
                lblPreview.Font = previewFont;
                lblPreview.Text = _previewText;
            }
            catch (Exception ex)
            {
                MessageBox.Show("미리보기 폰트 설정 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 설정 대상 변경 이벤트 핸들러
        /// </summary>
        private void cmbTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 현재 UI 상태를 이전 폰트에 저장
            UpdateCurrentFontFromUI();
            
            // 새로운 대상의 인덱스 저장
            _currentTargetIndex = cmbTarget.SelectedIndex;
            
            // 새로운 대상의 폰트 설정으로 UI 업데이트
            UpdateUIFromSettings();
        }

        /// <summary>
        /// 기본값 사용 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void btnUseDefault_Click(object sender, EventArgs e)
        {
            Font defaultFont = null;
            
            switch (_currentTargetIndex)
            {
                case 0: // 폴더창
                    defaultFont = new Font("굴림", 9);
                    break;
                case 1: // 파일뷰어창
                    defaultFont = new Font("굴림체", 10);
                    break;
                case 2: // 상태표시줄
                    defaultFont = new Font("굴림", 9);
                    break;
                case 3: // 주소표시줄
                    defaultFont = new Font("굴림", 9);
                    break;
            }
            
            if (defaultFont != null)
            {
                // 현재 선택된 대상에 따라 폰트 설정
                switch (_currentTargetIndex)
                {
                    case 0: // 폴더창
                        _folderFont = defaultFont;
                        break;
                    case 1: // 파일뷰어창
                        _viewerFont = defaultFont;
                        break;
                    case 2: // 상태표시줄
                        _statusFont = defaultFont;
                        break;
                    case 3: // 주소표시줄
                        _addressBarFont = defaultFont;
                        break;
                }
                
                // UI 업데이트
                _currentFont = defaultFont;
                UpdateUIFromSettings();
            }
        }
    }
}
