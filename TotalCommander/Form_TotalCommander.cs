using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using TotalCommander.GUI;

namespace TotalCommander
{
    public partial class Form_TotalCommander : Form
    {
        #region Fields
        private GUI.ShellBrowser m_PreviousFocus;
        private Font currentAppliedFont;
        private int _tabFocusState = 0;
        private KeySettings keySettings;
        private bool isRightVisible;
        #endregion

        public Form_TotalCommander()
        {
            InitializeComponent();
            isRightVisible = true;
            
            InitializeMenus();
            
            // 기능 키 설정 불러오기
            keySettings = KeySettings.Load();
            
            // 디버깅: F5 키 설정 로드 확인 (메시지 박스 대신 로그로 대체)
            KeyAction f5Action = keySettings.GetActionForKey(Keys.F5);
            string f5UserOption = keySettings.GetUserExecuteOptionNameForKey(Keys.F5);
            string f5Debug = $"시작 시 F5 키 설정: 액션={f5Action}";
            if (f5Action == KeyAction.UserExecute && !string.IsNullOrEmpty(f5UserOption))
            {
                f5Debug += $", 옵션={f5UserOption}";
                UserExecuteOption option = keySettings.GetUserExecuteOptionByName(f5UserOption);
                if (option != null)
                {
                    f5Debug += $"\n실행 파일: {option.ExecutablePath}";
                }
            }
            // 메시지 박스 대신 로그로 출력
            Logger.Info(f5Debug);
            
            // Initialize the static ImageList for all ShellBrowser instances FIRST.
            GUI.ShellBrowser.SmallImageList = new ImageList();
            GUI.ShellBrowser.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
            GUI.ShellBrowser.SmallImageList.ImageSize = new Size(16, 16);

            // Now, initialize ShellBrowsers and set initial focus
            shellBrowser_Left.Init();
            shellBrowser_Right.Init();
            m_PreviousFocus = shellBrowser_Left;
            
            // Store the initial font and load saved font settings
            currentAppliedFont = shellBrowser_Left.Font;
            LoadFontSettings();

            // Register focus events
            shellBrowser_Left.RecvFocus += Browser_GotFocus;
            shellBrowser_Right.RecvFocus += Browser_GotFocus;

            // Register status changed events
            shellBrowser_Left.StatusChanged += ShellBrowser_StatusChanged;
            shellBrowser_Right.StatusChanged += ShellBrowser_StatusChanged;

            // 키 이벤트를 폼 수준에서 처리하기 위한 설정
            this.KeyPreview = true;

            // Register KeyDown event
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_TotalCommander_KeyDown);

            // Set initial status
            UpdatePanelStatus(shellBrowser_Left);
            UpdatePanelStatus(shellBrowser_Right);
            
            // 디버깅 메시지: 설정 로드 확인
            string configInfo = "로드된 키 설정:\n";
            foreach (var setting in keySettings.Settings)
            {
                configInfo += $"{setting.Key}: {setting.Action}";
                if (setting.Action == KeyAction.UserExecute)
                    configInfo += $" ({setting.UserExecuteOptionName})";
                configInfo += "\n";
            }
            // 메시지 박스 대신 로그로 출력
            Logger.DebugMultiline("설정 로드 정보", configInfo);

            // 컬럼 설정 로드 및 적용
            LoadColumnSettings();
            
            // 컬럼 너비 변경 이벤트 처리
            shellBrowser_Left.RegisterColumnWidthChangedEvent(Browser_ColumnWidthChanged);
            shellBrowser_Right.RegisterColumnWidthChangedEvent(Browser_ColumnWidthChanged);
            
            // 분할 위치 변경 이벤트 처리
            shellBrowser_Left.RegisterSplitterDistanceChangedEvent(SplitterDistanceChanged);
            shellBrowser_Right.RegisterSplitterDistanceChangedEvent(SplitterDistanceChanged);

            // 메뉴 초기화
            InitializeMenus();
        }

        private void ShellBrowser_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            var browser = sender as ShellBrowser;
            if (browser != null)
            {
                browser.BottomStatusLabel.Text = e.StatusText;
            }
        }

        private void Panel_Enter(object sender, EventArgs e)
        {
            ShellBrowser browser = sender as ShellBrowser;
            if (browser != null)
            {
                UpdatePanelStatus(browser);
            }
        }

        private void UpdatePanelStatus(ShellBrowser browser)
        {
            browser.UpdateFolderSummaryStatus();
        }

        void Browser_GotFocus(object sender, EventArgs e)
        {
            GUI.ShellBrowser tmp = sender as GUI.ShellBrowser;
            if (tmp != null)
            {
                m_PreviousFocus = tmp;
            }
        }

        #region Bottom buttons
        private void Form_TotalCommander_KeyDown(object sender, KeyEventArgs e)
        {
            // 디버깅 메시지 - 어떤 키가 눌렸는지 확인
            //MessageBox.Show($"KeyDown 이벤트 발생: {e.KeyCode}", "키 입력 디버그", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 기능 키(F2~F8)인 경우 설정된 액션 실행
            if (e.KeyCode >= Keys.F2 && e.KeyCode <= Keys.F8)
            {
                // F5 키에 대한 특별 처리 - 사용자 실행 옵션이 있는지 확인
                if (e.KeyCode == Keys.F5)
                {
                    KeyAction action = keySettings.GetActionForKey(Keys.F5);
                    if (action == KeyAction.UserExecute)
                    {
                        // F5 키가 사용자 실행 옵션으로 설정된 경우
                        ExecuteKeyAction(Keys.F5);
                        e.Handled = true;
                        e.SuppressKeyPress = true; // 다른 이벤트 핸들러로 전파되지 않도록
                        return;
                    }
                }

                ExecuteKeyAction(e.KeyCode);
                e.Handled = true;
                return;
            }

            // 다른 키 처리
            switch (e.KeyData)
            {
                case Keys.Delete:
                    m_PreviousFocus.DeleteSelectedItems(Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    e.Handled = true;
                    break;
                case Keys.Shift | Keys.Delete:
                    m_PreviousFocus.DeleteSelectedItems(Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently);
                    e.Handled = true;
                    break;
                case Keys.Control | Keys.Q:
                    this.Close();
                    break;
                // Tab 키는 더 이상 여기서 처리하지 않고 ProcessTabKey 메서드에서만 처리합니다.
            }
        }

        /// <summary>
        /// 설정된 키 액션 실행
        /// </summary>
        private void ExecuteKeyAction(Keys key)
        {
            KeyAction action = keySettings.GetActionForKey(key);
            
            // 디버깅 메시지 추가
            //MessageBox.Show($"키: {key}, 액션: {action}", "디버그 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // UserExecute 액션이 있으면 우선 처리
            if (action == KeyAction.UserExecute)
            {
                ExecuteUserOption(key);
                return;
            }
            
            // 기본 액션 처리
            switch (action)
            {
                case KeyAction.None:
                    // 아무 작업 없음
                    break;
                case KeyAction.View:
                case KeyAction.Edit:
                    m_PreviousFocus.EditWithNotepad();
                    break;
                case KeyAction.Copy:
                    m_PreviousFocus.CopySelectedItems();
                    break;
                case KeyAction.Cut:
                    m_PreviousFocus.CutSelectedItems();
                    break;
                case KeyAction.Paste:
                    m_PreviousFocus.PasteFromClipboard();
                    break;
                case KeyAction.Delete:
                    m_PreviousFocus.DeleteSelectedItems(Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    break;
                case KeyAction.CreateFolder:
                    m_PreviousFocus.CreateNewFolder();
                    break;
                case KeyAction.Properties:
                    m_PreviousFocus.OpenPropertiesWindowWithSelectedItems();
                    break;
                case KeyAction.Refresh:
                    m_PreviousFocus.RefreshAll();
                    break;
                case KeyAction.GoParent:
                    m_PreviousFocus.GoParent();
                    break;
                case KeyAction.GoBack:
                    m_PreviousFocus.GoBackward();
                    break;
                case KeyAction.GoForward:
                    m_PreviousFocus.GoForward();
                    break;
                case KeyAction.Exit:
                    this.Close();
                    break;
                case KeyAction.Rename:
                    m_PreviousFocus.RenameSelectedItem();
                    break;
            }
        }

        private void ExecuteUserOption(Keys key)
        {
            Logger.Debug($"ExecuteUserOption called with key: {key}");
            
            UserExecuteOption option = keySettings.GetUserExecuteOptionForKey(key);
            if (option == null)
            {
                Logger.Warning($"No user execute option found for key: {key}");
                MessageBox.Show($"키 {key}에 지정된 사용자 실행 옵션을 찾을 수 없습니다.", "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Logger.Debug($"Found user execute option: {option.Name}");
            
            // 왼쪽/오른쪽 파일창에서 선택된 항목 경로 가져오기
            string leftExplorerFullPath = GetSelectedPathFromExplorer(shellBrowser_Left);
            string rightExplorerFullPath = GetSelectedPathFromExplorer(shellBrowser_Right);
            
            // 현재 포커싱된 파일표시창에서 선택된 항목 경로
            string focusingExplorerFullPath = GetSelectedPathFromExplorer(m_PreviousFocus);
            
            Logger.Debug($"Executing option '{option.Name}' with paths: Left={leftExplorerFullPath ?? "null"}, Right={rightExplorerFullPath ?? "null"}, Focus={focusingExplorerFullPath ?? "null"}");

            // 옵션 실행
            option.Execute(leftExplorerFullPath, rightExplorerFullPath, focusingExplorerFullPath);
        }

        private string GetSelectedPathFromExplorer(ShellBrowser explorer)
        {
            if (explorer == null)
            {
                Logger.Debug("GetSelectedPathFromExplorer: explorer is null");
                return null;
            }

            // 현재 탐색기의 선택된 항목이 없거나 잘못된 경우 현재 경로 반환
            if (explorer.FileExplorer.SelectedIndices.Count == 0)
            {
                Logger.Debug($"GetSelectedPathFromExplorer: No selected items. Returning current path: {explorer.CurrentPath}");
                return explorer.CurrentPath; // 항목이 선택되지 않았을 때 현재 디렉토리 경로 반환
            }
                
            try
            {
                int selectedIndex = explorer.FileExplorer.SelectedIndices[0];
                if (selectedIndex < 0 || selectedIndex >= explorer.m_ShellItemInfo.Count)
                {
                    Logger.Debug($"GetSelectedPathFromExplorer: Selected index {selectedIndex} is out of range. Returning current path: {explorer.CurrentPath}");
                    return explorer.CurrentPath; // 인덱스가 범위를 벗어날 경우 현재 디렉토리 경로 반환
                }
                    
                FileSystemInfo selectedItem = explorer.m_ShellItemInfo[selectedIndex];
                Logger.Debug($"GetSelectedPathFromExplorer: Selected item: {selectedItem.FullName}");
                return selectedItem.FullName;
            }
            catch (Exception ex)
            {
                // 오류 발생 시 현재 디렉토리 경로 반환
                Logger.Error(ex, "GetSelectedPathFromExplorer failed");
                MessageBox.Show($"선택된 항목 경로를 가져오는 중 오류 발생: {ex.Message}", "경로 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return explorer.CurrentPath;
            }
        }

        private void HandleTabKey()
        {
            // 모든 중간 상태를 제거하고 명시적으로 4가지 상태만 사용
            _tabFocusState = (_tabFocusState + 1) % 4;
            
            // 디버깅용 메시지 활성화 - 실제 상태 변화 확인
            // MessageBox.Show("Tab 상태: " + _tabFocusState);
            
            // 명시적으로 4가지 상태를 하드코딩
            switch (_tabFocusState)
            {
                case 0: // 좌측 트리
                    shellBrowser_Left.FocusNavigationPane();
                    break;
                case 1: // 좌측 파일 목록
                    shellBrowser_Left.FocusFileBrowser();
                    break;
                case 2: // 우측 트리
                    shellBrowser_Right.FocusNavigationPane();
                    break;
                case 3: // 우측 파일 목록
                    shellBrowser_Right.FocusFileBrowser();
                    break;
            }
        }

        // 중요: Tab 키를 처리하기 위해 ProcessTabKey 메서드를 오버라이드
        protected override bool ProcessTabKey(bool forward)
        {
            // Windows Forms의 기본 Tab 키 동작을 비활성화하고 우리의 커스텀 로직으로 대체
            HandleTabKey();
            return true; // Tab 키를 처리했음을 의미
        }
        
        // Tab 키를 더 확실하게 가로채기 위해 ProcessCmdKey 메서드도 오버라이드
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // F5 키 이벤트 처리
            if (keyData == Keys.F5)
            {
                // F5 키에 대한 특별 처리
                KeyAction action = keySettings.GetActionForKey(Keys.F5);
                
                if (action == KeyAction.UserExecute)
                {
                    try
                    {
                        UserExecuteOption option = keySettings.GetUserExecuteOptionForKey(Keys.F5);
                        if (option != null)
                        {
                            // 현재 선택된 항목 경로 가져오기
                            string explorer1SelectedPath = GetSelectedPathFromExplorer(shellBrowser_Left);
                            string explorer2SelectedPath = GetSelectedPathFromExplorer(shellBrowser_Right);
                            
                            // 옵션 직접 실행
                            option.Execute(explorer1SelectedPath, explorer2SelectedPath);
                            return true; // 이벤트 처리 완료
                        }
                        else
                        {
                            MessageBox.Show("설정된 사용자 실행 옵션을 찾을 수 없습니다.", 
                                           "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"사용자 실행 옵션 실행 중 오류: {ex.Message}", 
                                       "실행 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return true; // 오류가 발생했지만 이벤트는 처리됨
                }
                else if (action != KeyAction.None)
                {
                    // 다른 F5 키 액션 처리
                    ExecuteKeyAction(Keys.F5);
                    return true; // 이벤트 처리 완료
                }
            }
            
            // Tab 키 처리
            if (keyData == Keys.Tab)
            {
                HandleTabKey();
                return true;
            }
            
            // 다른 키는 기본 처리
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion Bottom buttons

        #region Menu items
        private void largeIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_PreviousFocus.ChangeViewMode(View.LargeIcon);
        }

        private void smallIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_PreviousFocus.ChangeViewMode(View.SmallIcon);
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_PreviousFocus.ChangeViewMode(View.List);
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_PreviousFocus.ChangeViewMode(View.Details);
        }

        private void tileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_PreviousFocus.ChangeViewMode(View.Tile);
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormFontSettings fontDialog = new FormFontSettings(currentAppliedFont))
            {
                if (fontDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Font selectedFont = fontDialog.SelectedFont;
                    if (selectedFont != null)
                    {
                        // 주 폰트 적용
                        shellBrowser_Left.ApplyFont(selectedFont);
                        shellBrowser_Right.ApplyFont(selectedFont);
                        currentAppliedFont = selectedFont;
                        
                        // 하단 상태창 폰트 설정
                        bool applyToStatusBar = fontDialog.ApplyToStatusBar;
                        if (applyToStatusBar)
                        {
                            shellBrowser_Left.ApplyStatusBarFont(selectedFont);
                            shellBrowser_Right.ApplyStatusBarFont(selectedFont);
                        }
                        
                        // 폰트 설정 저장
                        SaveFontSettings(selectedFont, applyToStatusBar);
                    }
                }
            }
        }

        // 폰트 설정을 저장하는 메서드
        private void SaveFontSettings(Font font, bool applyToStatusBar)
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
                
                // 디렉토리가 없으면 생성
                if (!Directory.Exists(tcFolder))
                    Directory.CreateDirectory(tcFolder);
                    
                string fontSettingsPath = Path.Combine(tcFolder, "FontSettings.xml");
                
                // 폰트 정보 XML로 저장
                using (StreamWriter writer = new StreamWriter(fontSettingsPath))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<FontSettings>");
                    writer.WriteLine($"  <FontFamily>{font.FontFamily.Name}</FontFamily>");
                    writer.WriteLine($"  <Size>{font.Size}</Size>");
                    writer.WriteLine($"  <Bold>{font.Bold}</Bold>");
                    writer.WriteLine($"  <Italic>{font.Italic}</Italic>");
                    writer.WriteLine($"  <Underline>{font.Underline}</Underline>");
                    writer.WriteLine($"  <Strikeout>{font.Strikeout}</Strikeout>");
                    writer.WriteLine($"  <ApplyToStatusBar>{applyToStatusBar}</ApplyToStatusBar>");
                    writer.WriteLine("</FontSettings>");
                }
                
                // 디버깅용 메시지 표시
                MessageBox.Show($"폰트 설정이 저장되었습니다: {fontSettingsPath}", 
                              "폰트 설정 저장", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폰트 설정을 저장하는 중 오류가 발생했습니다: {ex.Message}", 
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void functionKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFunctionKeySettings();
        }
        #endregion Menu items

        #region Device detector constants and Structs

        const int WM_DEVICECHANGE = 0x219;
        const int DBT_DEVICEARRIVAL = 0x8000;
        const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        const int DBT_DEVTYP_VOLUME = 0x00000002;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct DevBroadcastVolume
        {
            public int Size;
            public int DeviceType;
            public int Reserved;
            public int Mask;
            public Int16 Flags;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    switch ((int)m.WParam)
                    {
                        case DBT_DEVICEARRIVAL:
                        case DBT_DEVICEREMOVECOMPLETE:
                            shellBrowser_Left.OnDeviceDetected(null, null);
                            shellBrowser_Right.OnDeviceDetected(null, null);
                            break;
                    }
                    break;
            }
        }

        #endregion Device detector constants and Structs

        private void InitializeMenus()
        {
            // 기존 메뉴(mnuMain)에 메뉴 항목 추가
            if (configurationToolStripMenuItem != null)
            {
                // User Execute Options 메뉴 항목 추가 (아직 없다면)
                bool hasUserExecuteOptions = false;
                foreach (ToolStripItem item in configurationToolStripMenuItem.DropDownItems)
                {
                    if (item is ToolStripMenuItem && item.Text == "User Execute Options")
                    {
                        hasUserExecuteOptions = true;
                        break;
                    }
                }

                if (!hasUserExecuteOptions)
                {
                    ToolStripMenuItem userExecuteOptionsMenuItem = new ToolStripMenuItem("User Execute Options");
                    userExecuteOptionsMenuItem.Click += (s, e) => ShowUserExecuteOptions();
                    configurationToolStripMenuItem.DropDownItems.Add(userExecuteOptionsMenuItem);
                }
            }
        }

        private void ShowUserExecuteOptions()
        {
            using (FormManageUserOptions form = new FormManageUserOptions(keySettings))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // 설정이 변경되었으므로 다시 로드
                    KeySettings oldSettings = keySettings;
                    keySettings = KeySettings.Load();
                    
                    // 메시지로 변경 사항 표시
                    string message = "사용자 실행 옵션이 업데이트되었습니다.\n";
                    message += "F5 키 설정: " + keySettings.GetActionForKey(Keys.F5);
                    if (keySettings.GetActionForKey(Keys.F5) == KeyAction.UserExecute)
                    {
                        string optionName = keySettings.GetUserExecuteOptionNameForKey(Keys.F5);
                        message += $" ({optionName})";
                        UserExecuteOption option = keySettings.GetUserExecuteOptionByName(optionName);
                        if (option != null)
                        {
                            message += $"\n실행 파일: {option.ExecutablePath}";
                            message += $"\n매개변수: {option.Parameters}";
                        }
                    }
                    MessageBox.Show(message, "설정 업데이트", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ShowFunctionKeySettings()
        {
            using (FormKeySettings form = new FormKeySettings(keySettings))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // 설정이 변경되었으므로 다시 로드
                    KeySettings oldSettings = keySettings;
                    keySettings = KeySettings.Load();
                    
                    // 메시지로 변경 사항 표시
                    string message = "기능 키 설정이 업데이트되었습니다.\n";
                    message += "F5 키 설정: " + keySettings.GetActionForKey(Keys.F5);
                    if (keySettings.GetActionForKey(Keys.F5) == KeyAction.UserExecute)
                    {
                        string optionName = keySettings.GetUserExecuteOptionNameForKey(Keys.F5);
                        message += $" ({optionName})";
                        UserExecuteOption option = keySettings.GetUserExecuteOptionByName(optionName);
                        if (option != null)
                        {
                            message += $"\n실행 파일: {option.ExecutablePath}";
                            message += $"\n매개변수: {option.Parameters}";
                        }
                    }
                    MessageBox.Show(message, "설정 업데이트", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // 이동 관련 메서드들
        private void GoBack()
        {
            // 현재 활성화된 탐색기 패널 가져오기
            ShellBrowser activeExplorer = GetActiveExplorer();
            if (activeExplorer != null)
            {
                activeExplorer.GoBackward();
            }
        }

        private void GoForward()
        {
            // 현재 활성화된 탐색기 패널 가져오기
            ShellBrowser activeExplorer = GetActiveExplorer();
            if (activeExplorer != null)
            {
                activeExplorer.GoForward();
            }
        }

        private void GoToParentDirectory()
        {
            // 현재 활성화된 탐색기 패널 가져오기
            ShellBrowser activeExplorer = GetActiveExplorer();
            if (activeExplorer != null)
            {
                activeExplorer.GoParent();
            }
        }

        private ShellBrowser GetActiveExplorer()
        {
            // 현재 활성화된 탐색기 패널 반환
            // 왼쪽 패널이 포커스를 가지고 있으면 왼쪽, 아니면 오른쪽 반환
            return shellBrowser_Left.Focused ? shellBrowser_Left : shellBrowser_Right;
        }

        // 폰트 설정을 불러오는 메서드
        private void LoadFontSettings()
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
                string fontSettingsPath = Path.Combine(tcFolder, "FontSettings.xml");

                if (File.Exists(fontSettingsPath))
                {
                    string fontFamilyName = "Microsoft Sans Serif";
                    float fontSize = 9;
                    bool isBold = false;
                    bool isItalic = false;
                    bool isUnderline = false;
                    bool isStrikeout = false;
                    bool applyToStatusBar = true;

                    // XML 파일에서 폰트 설정 읽기
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fontSettingsPath);

                    XmlNode fontFamilyNode = doc.SelectSingleNode("//FontFamily");
                    if (fontFamilyNode != null)
                        fontFamilyName = fontFamilyNode.InnerText;

                    XmlNode sizeNode = doc.SelectSingleNode("//Size");
                    if (sizeNode != null)
                        fontSize = float.Parse(sizeNode.InnerText);

                    XmlNode boldNode = doc.SelectSingleNode("//Bold");
                    if (boldNode != null)
                        isBold = bool.Parse(boldNode.InnerText);

                    XmlNode italicNode = doc.SelectSingleNode("//Italic");
                    if (italicNode != null)
                        isItalic = bool.Parse(italicNode.InnerText);

                    XmlNode underlineNode = doc.SelectSingleNode("//Underline");
                    if (underlineNode != null)
                        isUnderline = bool.Parse(underlineNode.InnerText);

                    XmlNode strikeoutNode = doc.SelectSingleNode("//Strikeout");
                    if (strikeoutNode != null)
                        isStrikeout = bool.Parse(strikeoutNode.InnerText);
                        
                    XmlNode applyToStatusBarNode = doc.SelectSingleNode("//ApplyToStatusBar");
                    if (applyToStatusBarNode != null)
                        applyToStatusBar = bool.Parse(applyToStatusBarNode.InnerText);

                    // 폰트 스타일 설정
                    FontStyle style = FontStyle.Regular;
                    if (isBold) style |= FontStyle.Bold;
                    if (isItalic) style |= FontStyle.Italic;
                    if (isUnderline) style |= FontStyle.Underline;
                    if (isStrikeout) style |= FontStyle.Strikeout;

                    // 폰트 생성 및 적용
                    try
                    {
                        currentAppliedFont = new Font(fontFamilyName, fontSize, style);
                        shellBrowser_Left.ApplyFont(currentAppliedFont);
                        shellBrowser_Right.ApplyFont(currentAppliedFont);
                        
                        // 하단 상태창 폰트 설정
                        if (applyToStatusBar)
                        {
                            shellBrowser_Left.ApplyStatusBarFont(currentAppliedFont);
                            shellBrowser_Right.ApplyStatusBarFont(currentAppliedFont);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("폰트 적용 오류: " + ex.Message);
                        // 기본 폰트 사용
                        currentAppliedFont = SystemFonts.DefaultFont;
                    }
                }
                else
                {
                    // 파일이 없으면 기본 폰트 사용
                    currentAppliedFont = SystemFonts.DefaultFont;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("폰트 설정 로드 오류: " + ex.Message);
                // 오류 시 기본 폰트 사용
                currentAppliedFont = SystemFonts.DefaultFont;
            }
        }

        // 컬럼 너비 변경 이벤트 핸들러
        private void Browser_ColumnWidthChanged(object sender, EventArgs e)
        {
            // 컬럼 너비 변경 시 설정 저장
            SaveColumnSettings();
        }
        
        // 컬럼 설정을 저장하는 메서드
        private void SaveColumnSettings()
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
                
                // 디렉토리가 없으면 생성
                if (!Directory.Exists(tcFolder))
                    Directory.CreateDirectory(tcFolder);
                    
                string columnSettingsPath = Path.Combine(tcFolder, "ColumnSettings.xml");
                
                // 왼쪽, 오른쪽 패널의 컬럼 너비 가져오기
                Dictionary<int, int> leftColumnWidths = shellBrowser_Left.GetColumnWidths();
                Dictionary<int, int> rightColumnWidths = shellBrowser_Right.GetColumnWidths();
                
                // 분할 위치(SplitterDistance) 가져오기
                int leftSplitterDistance = shellBrowser_Left.GetSplitterDistance();
                int rightSplitterDistance = shellBrowser_Right.GetSplitterDistance();
                
                // 컬럼 정보 XML로 저장
                using (StreamWriter writer = new StreamWriter(columnSettingsPath))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<ColumnSettings>");
                    
                    // 왼쪽 패널 컬럼 설정
                    writer.WriteLine("  <LeftPanel>");
                    // 분할 위치 저장
                    writer.WriteLine($"    <SplitterDistance>{leftSplitterDistance}</SplitterDistance>");
                    foreach (var column in leftColumnWidths)
                    {
                        writer.WriteLine($"    <Column Index=\"{column.Key}\" Width=\"{column.Value}\" />");
                    }
                    writer.WriteLine("  </LeftPanel>");
                    
                    // 오른쪽 패널 컬럼 설정
                    writer.WriteLine("  <RightPanel>");
                    // 분할 위치 저장
                    writer.WriteLine($"    <SplitterDistance>{rightSplitterDistance}</SplitterDistance>");
                    foreach (var column in rightColumnWidths)
                    {
                        writer.WriteLine($"    <Column Index=\"{column.Key}\" Width=\"{column.Value}\" />");
                    }
                    writer.WriteLine("  </RightPanel>");
                    
                    writer.WriteLine("</ColumnSettings>");
                }
                
                // 디버깅용 메시지 (주석 처리)
                // System.Diagnostics.Debug.WriteLine("컬럼 설정이 저장되었습니다: " + columnSettingsPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("컬럼 설정 저장 오류: " + ex.Message);
            }
        }
        
        // 컬럼 설정을 불러오는 메서드
        private void LoadColumnSettings()
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
                string columnSettingsPath = Path.Combine(tcFolder, "ColumnSettings.xml");
                
                if (File.Exists(columnSettingsPath))
                {
                    // 왼쪽, 오른쪽 패널의 컬럼 너비를 저장할 딕셔너리
                    Dictionary<int, int> leftColumnWidths = new Dictionary<int, int>();
                    Dictionary<int, int> rightColumnWidths = new Dictionary<int, int>();
                    
                    // 분할 위치 기본값
                    int leftSplitterDistance = 0;
                    int rightSplitterDistance = 0;
                    
                    // XML 파일 로드
                    XmlDocument doc = new XmlDocument();
                    doc.Load(columnSettingsPath);
                    
                    // 왼쪽 패널 분할 위치 로드
                    XmlNode leftSplitterNode = doc.SelectSingleNode("//LeftPanel/SplitterDistance");
                    if (leftSplitterNode != null && int.TryParse(leftSplitterNode.InnerText, out leftSplitterDistance))
                    {
                        shellBrowser_Left.SetSplitterDistance(leftSplitterDistance);
                    }
                    
                    // 왼쪽 패널 컬럼 설정 로드
                    XmlNodeList leftColumns = doc.SelectNodes("//LeftPanel/Column");
                    if (leftColumns != null)
                    {
                        foreach (XmlNode column in leftColumns)
                        {
                            if (column.Attributes["Index"] != null && column.Attributes["Width"] != null)
                            {
                                int index = int.Parse(column.Attributes["Index"].Value);
                                int width = int.Parse(column.Attributes["Width"].Value);
                                leftColumnWidths[index] = width;
                            }
                        }
                    }
                    
                    // 오른쪽 패널 분할 위치 로드
                    XmlNode rightSplitterNode = doc.SelectSingleNode("//RightPanel/SplitterDistance");
                    if (rightSplitterNode != null && int.TryParse(rightSplitterNode.InnerText, out rightSplitterDistance))
                    {
                        shellBrowser_Right.SetSplitterDistance(rightSplitterDistance);
                    }
                    
                    // 오른쪽 패널 컬럼 설정 로드
                    XmlNodeList rightColumns = doc.SelectNodes("//RightPanel/Column");
                    if (rightColumns != null)
                    {
                        foreach (XmlNode column in rightColumns)
                        {
                            if (column.Attributes["Index"] != null && column.Attributes["Width"] != null)
                            {
                                int index = int.Parse(column.Attributes["Index"].Value);
                                int width = int.Parse(column.Attributes["Width"].Value);
                                rightColumnWidths[index] = width;
                            }
                        }
                    }
                    
                    // 설정 적용
                    shellBrowser_Left.ApplyColumnWidths(leftColumnWidths);
                    shellBrowser_Right.ApplyColumnWidths(rightColumnWidths);
                    
                    // 디버깅용 메시지 (주석 처리)
                    // System.Diagnostics.Debug.WriteLine("컬럼 설정이 로드되었습니다: " + columnSettingsPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("컬럼 설정 로드 오류: " + ex.Message);
            }
        }
        
        // 추가: SplitterDistance 변경 이벤트를 처리하는 메서드
        private void SplitterDistanceChanged(object sender, SplitterEventArgs e)
        {
            // 분할 위치가 변경되면 설정 저장
            SaveColumnSettings();
        }
    }
}
