using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using TotalCommander.GUI;
using System.Reflection;
using System.Xml.Serialization;
using System.Configuration;
using TotalCommander.Properties;
using System.Diagnostics;
using System.Xml;
// 커스텀 대화 상자 클래스 참조
using static TotalCommander.CustomDialogHelper;
using static TotalCommander.CustomMessageBox;
using System.IO.Compression;
using TotalCommander.Compression;

namespace TotalCommander
{
    public partial class Form_TotalCommander : Form
    {
        #region Fields
        private GUI.ShellBrowser m_PreviousFocus;
        private Font currentAppliedFont;
        private int _tabFocusState = 0;
        private KeySettings keySettings;
        private ContextMenuStrip ContextMenuFileOperations;
        #endregion

        public Form_TotalCommander()
        {
            InitializeComponent();

            // Show build date in title bar
            UpdateTitleWithBuildDateTime();
            
            // Load saved window position and size
            LoadWindowState();
            
            // Load function key settings
            keySettings = KeySettings.Load();
            
            // FormClosing 이벤트 핸들러 등록
            this.FormClosing += Form_TotalCommander_FormClosing;

            // Debug: Verify F5 key setting loaded (replace message box with log)
            KeyAction f5Action = keySettings.GetActionForKey(Keys.F5);
            string f5UserOption = keySettings.GetUserExecuteOptionNameForKey(Keys.F5);
            string f5Debug = StringResources.GetString("F5KeySettingAtStartup", f5Action);
            if (f5Action == KeyAction.UserExecute && !string.IsNullOrEmpty(f5UserOption))
            {
                f5Debug = StringResources.GetString("F5KeyWithOption", f5Action, f5UserOption);
                UserExecuteOption option = keySettings.GetUserExecuteOptionByName(f5UserOption);
                if (option != null)
                {
                    f5Debug += "\n" + StringResources.GetString("F5KeyExecutable", option.ExecutablePath);
                }
            }
            // Output to log instead of message box
            Logger.Information(f5Debug);
            
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

            // Set KeyPreview to handle key events at form level
            this.KeyPreview = true;

            // Register KeyDown event
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_TotalCommander_KeyDown);

            // Set initial status
            UpdatePanelStatus(shellBrowser_Left);
            UpdatePanelStatus(shellBrowser_Right);
            
            // Debug message: Verify loaded settings
            string configInfo = StringResources.GetString("LoadedKeySettings") + "\n";
            foreach (var setting in keySettings.Settings)
            {
                if (setting.Action == KeyAction.UserExecute)
                    configInfo += StringResources.GetString("KeySettingWithOption", setting.Key, setting.Action, setting.UserExecuteOptionName) + "\n";
                else
                    configInfo += StringResources.GetString("KeySettingFormat", setting.Key, setting.Action) + "\n";
            }
            // Output to log instead of message box
            Logger.DebugMultiline("Settings Load Info", configInfo);

            // Load and apply column settings
            LoadColumnSettings();
            
            // Handle column width changed events
            shellBrowser_Left.RegisterColumnWidthChangedEvent(Browser_ColumnWidthChanged);
            shellBrowser_Right.RegisterColumnWidthChangedEvent(Browser_ColumnWidthChanged);
            
            // Handle splitter distance changed events
            shellBrowser_Left.RegisterSplitterDistanceChangedEvent(SplitterDistanceChanged);
            shellBrowser_Right.RegisterSplitterDistanceChangedEvent(SplitterDistanceChanged);

            // Initialize context menus
            InitContextMenus();
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
                // 포커스 설정 로그 추가
                Logger.Debug($"Browser_GotFocus: Focus set to {(tmp == shellBrowser_Left ? "Left" : "Right")} explorer");
            }
        }

        #region Bottom buttons
        private void Form_TotalCommander_KeyDown(object sender, KeyEventArgs e)
        {
            // Add key event logging
            Logger.Debug(StringResources.GetString("DebugKeyEvent", e.KeyCode, e.KeyData, e.Handled));

            // For function keys (F2-F8), execute configured action
            if (e.KeyCode >= Keys.F2 && e.KeyCode <= Keys.F8)
            {
                // Special handling for F5 key - check if user execute option exists
                if (e.KeyCode == Keys.F5)
                {
                    KeyAction action = keySettings.GetActionForKey(Keys.F5);
                    if (action == KeyAction.UserExecute)
                    {
                        // If F5 is set to user execute option
                        ExecuteKeyAction(Keys.F5);
                        e.Handled = true;
                        e.SuppressKeyPress = true; // Prevent propagation to other event handlers
                        return;
                    }
                }

                ExecuteKeyAction(e.KeyCode);
                e.Handled = true;
                return;
            }

            // Handle other keys
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
                // Ctrl+Shift+Right Arrow: Copy from left to right
                case Keys.Control | Keys.Shift | Keys.Right:
                    // Use Shell32 API for copying
                    CopyBetweenPanels(shellBrowser_Left, shellBrowser_Right);
                    e.Handled = true;
                    break;
                // Ctrl+Shift+Left Arrow: Copy from right to left
                case Keys.Control | Keys.Shift | Keys.Left:
                    // Use Shell32 API for copying
                    CopyBetweenPanels(shellBrowser_Right, shellBrowser_Left);
                    e.Handled = true;
                    break;
                // Alt+D: Focus left address bar (depending on currently selected panel)
                case Keys.Alt | Keys.D:
                    if (m_PreviousFocus == shellBrowser_Left)
                    {
                        shellBrowser_Left.FocusAddressBar();
                    }
                    else if (m_PreviousFocus == shellBrowser_Right)
                    {
                        shellBrowser_Right.FocusAddressBar();
                    }
                    e.Handled = true;
                    break;
                // Ctrl+Shift+X: 압축 풀기
                case Keys.Control | Keys.Shift | Keys.X:
                    ExtractArchives();
                    e.Handled = true;
                    break;
                // Tab key is no longer handled here, it's handled in ProcessTabKey method only
            }
        }

        /// <summary>
        /// Execute configured key action
        /// </summary>
        private void ExecuteKeyAction(Keys key)
        {
            KeyAction action = keySettings.GetActionForKey(key);
            
            // Debug message addition
            //MessageBox.Show($"Key: {key}, Action: {action}", "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // If UserExecute action exists, handle it first
            if (action == KeyAction.UserExecute)
            {
                ExecuteUserOption(key);
                return;
            }
            
            // Default action handling
            switch (action)
            {
                case KeyAction.None:
                    // No action
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
            try
            {
                Logger.Debug($"ExecuteUserOption called with key: {key}");
                
                UserExecuteOption option = keySettings.GetUserExecuteOptionForKey(key);
                if (option == null)
                {
                    CustomDialogHelper.ShowMessageBox(this, $"No user execute option found for key: {key}", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Logger.Debug($"Found user execute option: {option.Name}");
                
                // Get selected item paths from left/right file panes
                string leftExplorerFullPath = GetSelectedPathFromExplorer(shellBrowser_Left);
                string rightExplorerFullPath = GetSelectedPathFromExplorer(shellBrowser_Right);
                
                // Get selected item path from currently focused file pane
                string focusingExplorerFullPath = GetSelectedPathFromExplorer(m_PreviousFocus);
                
                Logger.Debug($"Executing option '{option.Name}' with paths: Left={leftExplorerFullPath ?? "null"}, Right={rightExplorerFullPath ?? "null"}, Focus={focusingExplorerFullPath ?? "null"}");

                // Execute option
                option.Execute(leftExplorerFullPath, rightExplorerFullPath, focusingExplorerFullPath);
            }
            catch (Exception ex)
            {
                CustomDialogHelper.ShowMessageBox(this, $"Error executing user execute option: {ex.Message}", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSelectedPathFromExplorer(ShellBrowser explorer)
        {
            if (explorer == null)
            {
                Logger.Debug("GetSelectedPathFromExplorer: explorer is null");
                return null;
            }

            // If no selected items or incorrect, return current path
            if (explorer.FileExplorer.SelectedIndices.Count == 0)
            {
                Logger.Debug($"GetSelectedPathFromExplorer: No selected items. Returning current path: {explorer.CurrentPath}");
                return explorer.CurrentPath; // Return current directory path if no item is selected
            }
                
            try
            {
                int selectedIndex = explorer.FileExplorer.SelectedIndices[0];
                if (selectedIndex < 0 || selectedIndex >= explorer.m_ShellItemInfo.Count)
                {
                    Logger.Debug($"GetSelectedPathFromExplorer: Selected index {selectedIndex} is out of range. Returning current path: {explorer.CurrentPath}");
                    return explorer.CurrentPath; // Return current directory path if index is out of range
                }
                    
                FileSystemInfo selectedItem = explorer.m_ShellItemInfo[selectedIndex];
                Logger.Debug($"GetSelectedPathFromExplorer: Selected item: {selectedItem.FullName}");
                return selectedItem.FullName;
            }
            catch (Exception ex)
            {
                // If error occurs, return current directory path
                Logger.Error(ex, "GetSelectedPathFromExplorer failed");
                CustomDialogHelper.ShowMessageBox(this, $"Error getting selected item path: {ex.Message}", "Path Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return explorer.CurrentPath;
            }
        }

        private void HandleTabKey()
        {
            // Remove all intermediate states and explicitly use only 4 states
            _tabFocusState = (_tabFocusState + 1) % 4;
            
            // Debug message activation - confirm actual state change
            // MessageBox.Show("Tab state: " + _tabFocusState);
            
            // Explicitly hardcode 4 states
            switch (_tabFocusState)
            {
                case 0: // Left tree
                    shellBrowser_Left.FocusNavigationPane();
                    break;
                case 1: // Left file list
                    shellBrowser_Left.FocusFileBrowser();
                    break;
                case 2: // Right tree
                    shellBrowser_Right.FocusNavigationPane();
                    break;
                case 3: // Right file list
                    shellBrowser_Right.FocusFileBrowser();
                    break;
            }
        }

        // Important: Override ProcessTabKey method to handle Tab key
        protected override bool ProcessTabKey(bool forward)
        {
            // Disable default Tab key behavior and replace with our custom logic
            HandleTabKey();
            return true; // Indicates Tab key was handled
        }
        
        // Override ProcessCmdKey method to intercept Tab key more reliably
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Add key event logging
            Logger.Debug($"ProcessCmdKey: keyData={keyData}");
            
            // Handle Alt key to prevent menu activation
            if (keyData == (Keys.Alt) || keyData == (Keys.RMenu) || keyData == (Keys.LMenu))
            {
                return true;
            }
            
            // Space key is handled by file browser, pass it on
            if (keyData == Keys.Space)
            {
                Logger.Debug("ProcessCmdKey: Space key detected, event passed on");
                return false; // Pass event to other control
            }
            
            // Ctrl+Shift+X key for extracting archives
            if (keyData == (Keys.Control | Keys.Shift | Keys.X))
            {
                ExtractArchives();
                return true;
            }
            
            // F2 key event handling
            if (keyData == Keys.F2)
            {
                // Special handling for F2 key (Rename)
                KeyAction action = keySettings.GetActionForKey(Keys.F2);
                
                if (action == KeyAction.UserExecute)
                {
                    try
                    {
                        UserExecuteOption option = keySettings.GetUserExecuteOptionForKey(Keys.F2);
                        if (option != null)
                        {
                            // Get selected item path
                            string leftExplorerSelectedPath = GetSelectedPathFromExplorer(shellBrowser_Left);
                            string rightExplorerSelectedPath = GetSelectedPathFromExplorer(shellBrowser_Right);
                            string focusingExplorerSelectedPath = GetSelectedPathFromExplorer(m_PreviousFocus);

                            Logger.Debug($"ProcessCmdKey F2: Executing with Left={leftExplorerSelectedPath}, Right={rightExplorerSelectedPath}, Focus={focusingExplorerSelectedPath}");
                            
                            // Directly execute option
                            option.Execute(leftExplorerSelectedPath, rightExplorerSelectedPath, focusingExplorerSelectedPath);
                            return true; // Event processing completed
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomDialogHelper.ShowMessageBox(this, $"Error executing user execute option: {ex.Message}", 
                                       "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return true;
                }
                else if (action == KeyAction.Rename)
                {
                    // Handle rename action - call RenameSelectedItem on active explorer
                    if (m_PreviousFocus != null)
                    {
                        Logger.Debug("ProcessCmdKey F2: Executing Rename action");
                        m_PreviousFocus.RenameSelectedItem();
                        return true; // Event processing completed
                    }
                }
                else if (action != KeyAction.None)
                {
                    // Handle other F2 key actions
                    ExecuteKeyAction(Keys.F2);
                    return true; // Event processing completed
                }
            }
            
            // F5 key event handling
            if (keyData == Keys.F5)
            {
                // Special handling for F5 key
                KeyAction action = keySettings.GetActionForKey(Keys.F5);
                
                if (action == KeyAction.UserExecute)
                {
                    try
                    {
                        UserExecuteOption option = keySettings.GetUserExecuteOptionForKey(Keys.F5);
                        if (option != null)
                        {
                            // Get selected item path
                            string leftExplorerSelectedPath = GetSelectedPathFromExplorer(shellBrowser_Left);
                            string rightExplorerSelectedPath = GetSelectedPathFromExplorer(shellBrowser_Right);
                            string focusingExplorerSelectedPath = GetSelectedPathFromExplorer(m_PreviousFocus);

                            Logger.Debug($"ProcessCmdKey F5: Executing with Left={leftExplorerSelectedPath}, Right={rightExplorerSelectedPath}, Focus={focusingExplorerSelectedPath}");
                            
                            // Directly execute option
                            option.Execute(leftExplorerSelectedPath, rightExplorerSelectedPath, focusingExplorerSelectedPath);
                            return true; // Event processing completed
                        }
                        else
                        {
                            CustomDialogHelper.ShowMessageBox(this, "No configured user execute option found.", 
                                           "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomDialogHelper.ShowMessageBox(this, $"Error executing user execute option: {ex.Message}", 
                                       "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return true; // Error occurred but event is processed
                }
                else if (action != KeyAction.None)
                {
                    // Handle other F5 key actions
                    ExecuteKeyAction(Keys.F5);
                    return true; // Event processing completed
                }
            }
            
            // F12 key for settings
            if (keyData == Keys.F12)
            {
                ShowSettingsDialog();
                return true;
            }
            
            // Tab key handling
            if (keyData == Keys.Tab)
            {
                HandleTabKey();
                return true;
            }
            
            // Handle other keys with default processing
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        private void ShowSettingsDialog()
        {
            using (var settingsForm = new GUI.FormSettingsNew(this))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Settings already saved in form
                }
            }
        }
        
        // Methods called from FormSettings
        public void ShowFunctionKeysDialog()
        {
            ShowFunctionKeySettings();
        }
        
        public void ShowUserExecuteOptionsDialog()
        {
            // Show user execute options dialog
            ShowUserExecuteOptions();
        }
        
        public void ShowExplorerFontDialog()
        {
            // Show font dialog for explorer
            using (GUI.FormFontSettings fontSettings = new GUI.FormFontSettings(currentAppliedFont))
            {
                if (fontSettings.ShowDialog(this) == DialogResult.OK)
                {
                    // ApplyToStatusBar 속성 접근 전 리플렉션으로 존재 여부 확인
                    bool applyToStatusBar = true;
                    try
                    {
                        var prop = fontSettings.GetType().GetProperty("ApplyToStatusBar");
                        if (prop != null)
                        {
                            applyToStatusBar = (bool)prop.GetValue(fontSettings);
                        }
                    }
                    catch
                    {
                        // 예외 발생 시 기본값 사용
                        applyToStatusBar = true;
                    }

                    SaveFontSettings(fontSettings.SelectedFont, applyToStatusBar);
                }
            }
        }

        public void ShowStatusFontDialog()
        {
            // Show font dialog for status bar
            using (GUI.FormStatusFontSettings fontSettings = new GUI.FormStatusFontSettings(currentAppliedFont))
            {
                if (fontSettings.ShowDialog(this) == DialogResult.OK)
                {
                    SaveFontSettings(fontSettings.SelectedFont, true);
                }
            }
        }

        public void ApplySortMode(int sortMode)
        {
            shellBrowser_Left.ApplySortMode(sortMode);
            shellBrowser_Right.ApplySortMode(sortMode);
        }

        /// <summary>
        /// 설정이 변경되었을 때 호출되는 메서드
        /// </summary>
        public void SettingsChanged()
        {
            // 설정 변경 시 필요한 작업 수행
            // 정렬 모드 적용
            ApplySortMode(Properties.Settings.Default.DefaultSortMode);
            
            // 보기 설정 적용
            bool showHidden = Properties.Settings.Default.ShowHiddenFiles;
            bool showSystem = Properties.Settings.Default.ShowSystemFiles;
            shellBrowser_Left.SetShowHiddenSystemFiles(showHidden, showSystem);
            shellBrowser_Right.SetShowHiddenSystemFiles(showHidden, showSystem);
            
            // 전체 행 선택 설정
            bool fullRowSelect = Properties.Settings.Default.FullRowSelect;
            shellBrowser_Left.SetFullRowSelect(fullRowSelect);
            shellBrowser_Right.SetFullRowSelect(fullRowSelect);
            
            // 기타 설정 적용
            LoadFontSettings();
            
            // 브라우저 새로고침
            shellBrowser_Left.RefreshView();
            shellBrowser_Right.RefreshView();
        }

        // Method to save font settings
        private void SaveFontSettings(Font font, bool applyToStatusBar)
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(tcFolder))
                    Directory.CreateDirectory(tcFolder);
                    
                string fontSettingsPath = Path.Combine(tcFolder, "FontSettings.xml");
                
                // Save font settings as XML
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
                
                // Debug message display
                CustomDialogHelper.ShowMessageBox(this, $"Font settings saved: {fontSettingsPath}", 
                              "Font Settings Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                CustomDialogHelper.ShowMessageBox(this, $"Error saving font settings: {ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion Bottom buttons

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
            // 메뉴가 제거되었으므로 이 메서드는 비워둡니다.
            // 필요한 경우 새로운 메뉴 초기화 코드를 여기에 추가할 수 있습니다.
        }

        private void ShowUserExecuteOptions()
        {
            using (FormManageUserOptions form = new FormManageUserOptions(keySettings))
            {
                if (ShowDialogCentered(form) == DialogResult.OK)
                {
                    // Settings changed, reload again
                    KeySettings oldSettings = keySettings;
                    keySettings = KeySettings.Load();
                    
                    // Log message about changes without showing dialog
                    string message = "User execute options updated.\n";
                    message += "F5 key setting: " + keySettings.GetActionForKey(Keys.F5);
                    if (keySettings.GetActionForKey(Keys.F5) == KeyAction.UserExecute)
                    {
                        string optionName = keySettings.GetUserExecuteOptionNameForKey(Keys.F5);
                        message += $" ({optionName})";
                        UserExecuteOption option = keySettings.GetUserExecuteOptionByName(optionName);
                        if (option != null)
                        {
                            message += $"\nExecutable path: {option.ExecutablePath}";
                            message += $"\nParameters: {option.Parameters}";
                        }
                    }
                    Logger.Information(message);
                }
            }
        }

        private void ShowFunctionKeySettings()
        {
            using (FormKeySettings form = new FormKeySettings(keySettings))
            {
                if (ShowDialogCentered(form) == DialogResult.OK)
                {
                    // Settings changed, reload again
                    KeySettings oldSettings = keySettings;
                    keySettings = KeySettings.Load();
                    
                    // Log message about changes without showing dialog
                    string message = "Function key settings updated.\n";
                    message += "F5 key setting: " + keySettings.GetActionForKey(Keys.F5);
                    if (keySettings.GetActionForKey(Keys.F5) == KeyAction.UserExecute)
                    {
                        string optionName = keySettings.GetUserExecuteOptionNameForKey(Keys.F5);
                        message += $" ({optionName})";
                        UserExecuteOption option = keySettings.GetUserExecuteOptionByName(optionName);
                        if (option != null)
                        {
                            message += $"\nExecutable path: {option.ExecutablePath}";
                            message += $"\nParameters: {option.Parameters}";
                        }
                    }
                    Logger.Information(message);
                }
            }
        }

        // Move-related methods
        private void GoBack()
        {
            // Get active explorer panel
            ShellBrowser activeExplorer = GetActiveExplorer();
            if (activeExplorer != null)
            {
                activeExplorer.GoBackward();
            }
        }

        private void GoForward()
        {
            // Get active explorer panel
            ShellBrowser activeExplorer = GetActiveExplorer();
            if (activeExplorer != null)
            {
                activeExplorer.GoForward();
            }
        }

        private void GoToParentDirectory()
        {
            // Get active explorer panel
            ShellBrowser activeExplorer = GetActiveExplorer();
            if (activeExplorer != null)
            {
                activeExplorer.GoParent();
            }
        }

        private ShellBrowser GetActiveExplorer()
        {
            // Return active explorer panel
            // If left panel has focus, return left, otherwise return right
            return shellBrowser_Left.Focused ? shellBrowser_Left : shellBrowser_Right;
        }

        // Method to load font settings
        private void LoadFontSettings()
        {
            try
            {
                // 폴더창 폰트 로드 및 적용
                Font folderFont = LoadFontFromSettings("FolderFont", "굴림", 9);
                shellBrowser_Left.ApplyFont(folderFont);
                shellBrowser_Right.ApplyFont(folderFont);
                
                // 파일뷰어창 폰트 로드 및 적용
                Font viewerFont = LoadFontFromSettings("ViewerFont", "굴림체", 10);
                // TODO: 파일뷰어창이 구현되어 있다면 여기서 적용
                
                // 상태표시줄 폰트 로드 및 적용
                Font statusFont = LoadFontFromSettings("StatusFont", "굴림", 9);
                shellBrowser_Left.ApplyStatusBarFont(statusFont);
                shellBrowser_Right.ApplyStatusBarFont(statusFont);
                
                // 주소표시줄 폰트 로드 및 적용
                Font addressBarFont = LoadFontFromSettings("AddressBarFont", "굴림", 9);
                shellBrowser_Left.ApplyAddressBarFont(addressBarFont);
                shellBrowser_Right.ApplyAddressBarFont(addressBarFont);
                
                // 현재 적용된 폰트 저장 (호환성 유지)
                currentAppliedFont = folderFont;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Font settings load error: " + ex.Message);
                // 오류 발생 시 기본 폰트 사용
                currentAppliedFont = SystemFonts.DefaultFont;
            }
        }
        
        /// <summary>
        /// 설정에서 폰트 로드
        /// </summary>
        private Font LoadFontFromSettings(string settingName, string defaultFontName, float defaultSize)
        {
            try
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
                    return GUI.Settings.FontConverter.FromString(fontString);
                else
                    return new Font(defaultFontName, defaultSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font load error ({settingName}): {ex.Message}");
                return new Font(defaultFontName, defaultSize);
            }
        }

        // Column width changed event handler
        private void Browser_ColumnWidthChanged(object sender, EventArgs e)
        {
            // Save settings when column width changes
            SaveColumnSettings();
        }
        
        // Method to save column settings
        private void SaveColumnSettings()
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(tcFolder))
                    Directory.CreateDirectory(tcFolder);
                    
                string columnSettingsPath = Path.Combine(tcFolder, "ColumnSettings.xml");
                
                // Get column widths from left/right panels
                Dictionary<int, int> leftColumnWidths = shellBrowser_Left.GetColumnWidths();
                Dictionary<int, int> rightColumnWidths = shellBrowser_Right.GetColumnWidths();
                
                // Get splitter distances
                int leftSplitterDistance = shellBrowser_Left.GetSplitterDistance();
                int rightSplitterDistance = shellBrowser_Right.GetSplitterDistance();
                
                // Save column information as XML
                using (StreamWriter writer = new StreamWriter(columnSettingsPath))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<ColumnSettings>");
                    
                    // Left panel column settings
                    writer.WriteLine("  <LeftPanel>");
                    // Save splitter distance
                    writer.WriteLine($"    <SplitterDistance>{leftSplitterDistance}</SplitterDistance>");
                    foreach (var column in leftColumnWidths)
                    {
                        writer.WriteLine($"    <Column Index=\"{column.Key}\" Width=\"{column.Value}\" />");
                    }
                    writer.WriteLine("  </LeftPanel>");
                    
                    // Right panel column settings
                    writer.WriteLine("  <RightPanel>");
                    // Save splitter distance
                    writer.WriteLine($"    <SplitterDistance>{rightSplitterDistance}</SplitterDistance>");
                    foreach (var column in rightColumnWidths)
                    {
                        writer.WriteLine($"    <Column Index=\"{column.Key}\" Width=\"{column.Value}\" />");
                    }
                    writer.WriteLine("  </RightPanel>");
                    
                    writer.WriteLine("</ColumnSettings>");
                }
                
                // Debug message (commented out)
                // System.Diagnostics.Debug.WriteLine("Column settings saved: " + columnSettingsPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Column settings save error: " + ex.Message);
            }
        }
        
        // Method to load column settings
        private void LoadColumnSettings()
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
                string columnSettingsPath = Path.Combine(tcFolder, "ColumnSettings.xml");
                
                if (File.Exists(columnSettingsPath))
                {
                    // Dictionary to store column widths for left/right panels
                    Dictionary<int, int> leftColumnWidths = new Dictionary<int, int>();
                    Dictionary<int, int> rightColumnWidths = new Dictionary<int, int>();
                    
                    // Default splitter distances
                    int leftSplitterDistance = 0;
                    int rightSplitterDistance = 0;
                    
                    // Load XML file
                    XmlDocument doc = new XmlDocument();
                    doc.Load(columnSettingsPath);
                    
                    // Load left panel splitter distance
                    XmlNode leftSplitterNode = doc.SelectSingleNode("//LeftPanel/SplitterDistance");
                    if (leftSplitterNode != null && int.TryParse(leftSplitterNode.InnerText, out leftSplitterDistance))
                    {
                        shellBrowser_Left.SetSplitterDistance(leftSplitterDistance);
                    }
                    
                    // Load left panel column settings
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
                    
                    // Load right panel splitter distance
                    XmlNode rightSplitterNode = doc.SelectSingleNode("//RightPanel/SplitterDistance");
                    if (rightSplitterNode != null && int.TryParse(rightSplitterNode.InnerText, out rightSplitterDistance))
                    {
                        shellBrowser_Right.SetSplitterDistance(rightSplitterDistance);
                    }
                    
                    // Load right panel column settings
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
                    
                    // Apply settings
                    shellBrowser_Left.ApplyColumnWidths(leftColumnWidths);
                    shellBrowser_Right.ApplyColumnWidths(rightColumnWidths);
                    
                    // Debug message (commented out)
                    // System.Diagnostics.Debug.WriteLine("Column settings loaded: " + columnSettingsPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Column settings load error: " + ex.Message);
            }
        }
        
        // Additional: Method to handle SplitterDistance changed events
        private void SplitterDistanceChanged(object sender, SplitterEventArgs e)
        {
            // Save settings when splitter distance changes
            SaveColumnSettings();
        }

        private void UpdateTitleWithBuildDateTime()
        {
            try
            {
                // Get assembly file path
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                DateTime buildDateTime = File.GetLastWriteTime(assemblyPath);
                
                // Get version information
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                
                // Show build date and version in title bar
                this.Text = $"Total Commander - v{version} (Build: {buildDateTime:yyyy-MM-dd HH:mm:ss})";
                
                // Log to file
                Logger.Information($"Application version: {version}, Build date: {buildDateTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                // Use default title if error occurs
                this.Text = "Total Commander";
                Logger.Error(ex, "Build date retrieval failed");
            }
        }

        /// <summary>
        /// Copy selected files/folders between two panels (using Windows Shell32 API)
        /// </summary>
        /// <param name="source">Source ShellBrowser (panel with selected items)</param>
        /// <param name="destination">Destination ShellBrowser (panel with target path)</param>
        private void CopyBetweenPanels(ShellBrowser source, ShellBrowser destination)
        {
            try
            {
                if (source == null || destination == null)
                    return;
                
                // Get selected files from source panel
                List<string> selectedPaths = new List<string>();
                
                foreach (int index in source.FileExplorer.SelectedIndices)
                {
                    if (index >= 0 && index < source.m_ShellItemInfo.Count)
                    {
                        selectedPaths.Add(source.m_ShellItemInfo[index].FullName);
                    }
                }
                
                if (selectedPaths.Count == 0)
                    return;
                
                // Get destination path
                string destPath = destination.CurrentPath;
                if (string.IsNullOrEmpty(destPath))
                {
                    CustomDialogHelper.ShowMessageBox(this, "Destination path is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Show confirmation dialog
                FormCopyConfirm confirmDialog = new FormCopyConfirm(selectedPaths.ToArray(), destPath);
                if (ShowDialogCentered(confirmDialog) != DialogResult.OK)
                {
                    return;
                }
                
                // 진행 상태 대화상자 생성
                GUI.FormProgress progressForm = new GUI.FormProgress(selectedPaths.ToArray(), destPath, false);
                progressForm.OperationCompleted += ProgressForm_OperationCompleted;
                progressForm.Show(this);
                
                // Set up ShellFileOperation for copying
                ShellFileOperation fileOperation = new ShellFileOperation();
                fileOperation.SourceFiles = selectedPaths.ToArray();
                fileOperation.DestinationFolder = destPath;
                fileOperation.Operation = ShellFileOperation.FileOperations.FO_COPY;
                
                // Set operation options (show progress dialog)
                fileOperation.OperationFlags = 
                    ShellFileOperation.ShellFileOperationFlags.FOF_ALLOWUNDO | 
                    ShellFileOperation.ShellFileOperationFlags.FOF_NOCONFIRMMKDIR;
                
                // Execute operation
                bool success = fileOperation.DoOperation();
                
                if (success)
                {
                    // 복사 완료 후 두 패널 모두 새로고침
                    source.RefreshAll();
                    destination.RefreshAll();
                    
                    // Show copy completion message
                    string message = selectedPaths.Count == 1 
                        ? "1 item copied." 
                        : $"{selectedPaths.Count} items copied.";
                    destination.SetStatusMessage(message);
                    
                    Logger.Information("Shell32 API panel-to-panel copy completed");
                }
                else
                {
                    Logger.Error("Shell32 API panel-to-panel copy failed");
                    CustomDialogHelper.ShowMessageBox(this, "Copy operation did not complete.", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Shell32 API panel-to-panel copy error: {ex.Message}");
                CustomDialogHelper.ShowMessageBox(this, $"Copy error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Windows Shell32 API class for use
        /// </summary>
        private class ShellFileOperation
        {
            [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
            private static extern int SHFileOperation([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] ref SHFILEOPSTRUCT lpFileOp);

            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
            private struct SHFILEOPSTRUCT
            {
                public IntPtr hwnd;
                public FileOperations wFunc;
                [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public string pFrom;
                [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public string pTo;
                public ShellFileOperationFlags fFlags;
                [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
                public bool fAnyOperationsAborted;
                public IntPtr hNameMappings;
                [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
                public string lpszProgressTitle;
            }

            public enum FileOperations : uint
            {
                FO_MOVE = 0x0001,
                FO_COPY = 0x0002,
                FO_DELETE = 0x0003,
                FO_RENAME = 0x0004
            }

            [System.Flags]
            public enum ShellFileOperationFlags : ushort
            {
                FOF_MULTIDESTFILES = 0x0001,
                FOF_CONFIRMMOUSE = 0x0002,
                FOF_SILENT = 0x0004,
                FOF_RENAMEONCOLLISION = 0x0008,
                FOF_NOCONFIRMATION = 0x0010,
                FOF_WANTMAPPINGHANDLE = 0x0020,
                FOF_ALLOWUNDO = 0x0040,
                FOF_FILESONLY = 0x0080,
                FOF_SIMPLEPROGRESS = 0x0100,
                FOF_NOCONFIRMMKDIR = 0x0200,
                FOF_NOERRORUI = 0x0400,
                FOF_NOCOPYSECURITYATTRIBS = 0x0800,
                FOF_NORECURSION = 0x1000,
                FOF_NO_CONNECTED_ELEMENTS = 0x2000,
                FOF_WANTNUKEWARNING = 0x4000,
                FOF_NORECURSEREPARSE = 0x8000
            }

            // Source file list
            public string[] SourceFiles { get; set; }
            
            // Destination folder
            public string DestinationFolder { get; set; }
            
            // Operation type (copy, move, etc.)
            public FileOperations Operation { get; set; }
            
            // Operation options flags
            public ShellFileOperationFlags OperationFlags { get; set; }
            
            /// <summary>
            /// Execute file operation
            /// </summary>
            /// <returns>Success status</returns>
            public bool DoOperation()
            {
                if (SourceFiles == null || SourceFiles.Length == 0 || string.IsNullOrEmpty(DestinationFolder))
                {
                    return false;
                }
                
                try
                {
                    SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT();
                    fileOp.hwnd = IntPtr.Zero;
                    fileOp.wFunc = Operation;
                    
                    // Convert source file list to single string separated by null characters
                    StringBuilder sourceBuilder = new StringBuilder();
                    foreach (string file in SourceFiles)
                    {
                        sourceBuilder.Append(file);
                        sourceBuilder.Append('\0'); // null character to separate files
                    }
                    sourceBuilder.Append('\0'); // Add final null character
                    fileOp.pFrom = sourceBuilder.ToString();
                    
                    // Destination folder
                    fileOp.pTo = DestinationFolder + '\0' + '\0'; // Need double null characters for folder
                    
                    // Operation options
                    fileOp.fFlags = OperationFlags;
                    
                    // Execute operation
                    int result = SHFileOperation(ref fileOp);
                    
                    // Result check (0 is success)
                    return result == 0 && !fileOp.fAnyOperationsAborted;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Shell32 API error: {ex.Message}");
                    return false;
                }
            }
        }

        // Add this method to the class
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Subscribe to Application.Idle event to intercept form opening
            Application.AddMessageFilter(new FormCenteringMessageFilter(this));
        }
        
        /// <summary>
        /// Shows a dialog centered on the main form
        /// </summary>
        /// <param name="dialog">The dialog to show</param>
        /// <returns>The dialog result</returns>
        public DialogResult ShowDialogCentered(Form dialog)
        {
            return FormHelper.ShowDialogCentered(dialog, this);
        }

        private void Form_TotalCommander_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save window state before closing
            SaveWindowState();
        }

        private void SaveWindowState()
        {
            // Save maximized state separately
            if (this.WindowState == FormWindowState.Maximized)
            {
                Properties.Settings.Default.WindowMaximized = true;
            }
            else
            {
                // Only save size and position when the window is in normal state
                Properties.Settings.Default.WindowMaximized = false;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.WindowHeight = this.Height;
                Properties.Settings.Default.WindowLeft = this.Left;
                Properties.Settings.Default.WindowTop = this.Top;
            }
            
            // Save settings
            Properties.Settings.Default.Save();
            Logger.Debug("Window state saved");
        }

        private void LoadWindowState()
        {
            try
            {
                // Check if previously saved as maximized
                if (Properties.Settings.Default.WindowMaximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                    Logger.Debug("Window loaded in maximized state");
                    return;
                }
                
                // Check if saved size is valid
                if (Properties.Settings.Default.WindowWidth > 0 && 
                    Properties.Settings.Default.WindowHeight > 0)
                {
                    this.Width = Properties.Settings.Default.WindowWidth;
                    this.Height = Properties.Settings.Default.WindowHeight;
                    
                    // Check if saved position is valid
                    if (Properties.Settings.Default.WindowLeft > -10000 && 
                        Properties.Settings.Default.WindowTop > -10000)
                    {
                        this.Left = Properties.Settings.Default.WindowLeft;
                        this.Top = Properties.Settings.Default.WindowTop;
                    }
                    
                    // Verify window is visible on screen and adjust if necessary
                    EnsureVisibleOnScreen();
                    
                    Logger.Debug("Window loaded with saved size and position");
                }
                else
                {
                    // Default settings (normal state and centered)
                    this.WindowState = FormWindowState.Normal;
                    this.StartPosition = FormStartPosition.CenterScreen;
                    Logger.Debug("Window loaded with default state");
                }
            }
            catch (Exception ex)
            {
                // Set default state on error
                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.CenterScreen;
                Logger.Error("Error loading window state: " + ex.Message);
            }
        }
        
        private void EnsureVisibleOnScreen()
        {
            // Check if window is visible on screen and adjust position if needed
            Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
            bool needRepositioning = false;
            
            // Check if form is completely outside the screen
            if (this.Left + this.Width < screenBounds.Left + 50 || 
                this.Left > screenBounds.Right - 50 ||
                this.Top + this.Height < screenBounds.Top + 50 ||
                this.Top > screenBounds.Bottom - 50)
            {
                needRepositioning = true;
            }
            
            if (needRepositioning)
            {
                // Center on screen
                this.Left = screenBounds.Left + (screenBounds.Width - this.Width) / 2;
                this.Top = screenBounds.Top + (screenBounds.Height - this.Height) / 2;
                Logger.Debug("Window position adjusted to ensure visibility on screen");
            }
        }

        /// <summary>
        /// 압축 파일 확장자 확인
        /// </summary>
        private bool IsArchiveFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
                
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".zip" || extension == ".rar" || extension == ".7z" || 
                   extension == ".tar" || extension == ".gz" || extension == ".bz2";
        }
        
        /// <summary>
        /// 선택된 압축 파일들의 압축을 풉니다.
        /// </summary>
        private void ExtractArchives()
        {
            try
            {
                // 현재 포커스된 파일 브라우저의 선택된 파일 확인
                if (m_PreviousFocus?.FileExplorer?.SelectedIndices == null || 
                    m_PreviousFocus.FileExplorer.SelectedIndices.Count == 0)
                {
                    return;
                }
                
                // 선택된 모든 파일 가져오기
                List<FileSystemInfo> selectedFiles = new List<FileSystemInfo>();
                foreach (int index in m_PreviousFocus.FileExplorer.SelectedIndices)
                {
                    if (index >= 0 && index < m_PreviousFocus.m_ShellItemInfo.Count)
                    {
                        FileSystemInfo item = m_PreviousFocus.m_ShellItemInfo[index];
                        if (item is FileInfo fileInfo && IsArchiveFile(fileInfo.FullName))
                        {
                            selectedFiles.Add(fileInfo);
                        }
                    }
                }
                
                // 선택된 파일이 없거나, 압축 파일과 일반 파일이 혼합되어 있는 경우
                if (selectedFiles.Count == 0 || 
                    selectedFiles.Count != m_PreviousFocus.FileExplorer.SelectedIndices.Count)
                {
                    CustomDialogHelper.ShowMessageBox(this, 
                        "선택된 압축 파일이 없거나, 압축 파일과 일반 파일이 함께 선택되어 있습니다.", 
                        "압축 풀기", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 진행 상태 대화상자 생성
                GUI.FormProgress progressForm = new GUI.FormProgress(selectedFiles.Select(f => f.FullName).ToArray(), GUI.FormProgress.OperationType.Extract);
                progressForm.OperationCompleted += ProgressForm_OperationCompleted;
                progressForm.Show(this);
                
                // 압축 해제 작업 비동기 실행
                Task.Run(() =>
                {
                    int totalFiles = selectedFiles.Count;
                    int processedFiles = 0;
                    
                    try
                    {
                        // 압축 풀기 진행
                        foreach (FileInfo archiveFile in selectedFiles.Cast<FileInfo>())
                        {
                            if (progressForm.IsCancelled)
                                break;
                                
                            // 진행 상태 업데이트
                            progressForm.SetStatus($"압축 파일 처리 중: {archiveFile.Name}");
                            progressForm.SetCurrentFile($"파일: {archiveFile.Name}");
                            progressForm.UpdateFileProgress(processedFiles, totalFiles);
                            
                            // 압축 파일 이름과 동일한 폴더 생성 (확장자 제외)
                            string extractFolderName = Path.GetFileNameWithoutExtension(archiveFile.Name);
                            string extractPath = Path.Combine(archiveFile.DirectoryName, extractFolderName);
                            
                            // 폴더가 없으면 생성
                            if (!Directory.Exists(extractPath))
                            {
                                Directory.CreateDirectory(extractPath);
                            }
                            
                            try
                            {
                                // 압축 해제 - ZIP 파일은 .NET 내장 라이브러리로 처리
                                if (archiveFile.Extension.ToLowerInvariant() == ".zip")
                                {
                                    System.IO.Compression.ZipFile.ExtractToDirectory(archiveFile.FullName, extractPath);
                                }
                                else if (archiveFile.Extension.ToLowerInvariant() == ".7z" || 
                                        archiveFile.Extension.ToLowerInvariant() == ".tar" ||
                                        archiveFile.Extension.ToLowerInvariant() == ".gz" ||
                                        archiveFile.Extension.ToLowerInvariant() == ".bz2")
                                {
                                    // 7za.exe를 사용하여 압축 해제
                                    string sevenZipPath = Path.Combine(
                                        Path.GetDirectoryName(Application.ExecutablePath),
                                        "7za.exe");
                                        
                                    if (File.Exists(sevenZipPath))
                                    {
                                        ProcessStartInfo psi = new ProcessStartInfo
                                        {
                                            FileName = sevenZipPath,
                                            Arguments = $"x \"{archiveFile.FullName}\" -o\"{extractPath}\" -y",
                                            CreateNoWindow = true,
                                            UseShellExecute = false,
                                            RedirectStandardOutput = true,
                                            RedirectStandardError = true
                                        };
                                        
                                        using (Process process = new Process())
                                        {
                                            process.StartInfo = psi;
                                            process.OutputDataReceived += (s, e) => 
                                            {
                                                if (!string.IsNullOrEmpty(e.Data) && e.Data.Contains("%"))
                                                {
                                                    // 진행률 추출 및 업데이트
                                                    try
                                                    {
                                                        int percentIndex = e.Data.IndexOf("%");
                                                        if (percentIndex > 0)
                                                        {
                                                            int i = percentIndex - 1;
                                                            while (i >= 0 && char.IsDigit(e.Data[i])) i--;
                                                            string percentStr = e.Data.Substring(i + 1, percentIndex - i - 1);
                                                            if (int.TryParse(percentStr, out int percent))
                                                            {
                                                                // 진행률 표시는 필요 없음 (마퀴 스타일 인디케이터)
                                                                // 대신 현재 파일 정보 표시
                                                                string fileInfo = e.Data.Trim();
                                                                progressForm.SetCurrentFile(fileInfo);
                                                            }
                                                        }
                                                    }
                                                    catch { /* 무시 */ }
                                                }
                                            };
                                            
                                            process.Start();
                                            process.BeginOutputReadLine();
                                            process.WaitForExit();
                                            
                                            if (process.ExitCode != 0)
                                            {
                                                this.Invoke(new Action(() =>
                                                {
                                                    CustomDialogHelper.ShowMessageBox(this, 
                                                        $"'{archiveFile.Name}' 파일 압축 해제 중 오류가 발생했습니다.", 
                                                        "압축 풀기 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 기타 압축 파일은 외부 도구 실행 (PowerShell)
                                        ProcessStartInfo psi = new ProcessStartInfo
                                        {
                                            FileName = "powershell",
                                            Arguments = $"-Command \"Expand-Archive -Path '{archiveFile.FullName}' -DestinationPath '{extractPath}' -Force\"",
                                            CreateNoWindow = true,
                                            UseShellExecute = false
                                        };
                                        
                                        using (Process process = Process.Start(psi))
                                        {
                                            process.WaitForExit();
                                            if (process.ExitCode != 0)
                                            {
                                                this.Invoke(new Action(() =>
                                                {
                                                    CustomDialogHelper.ShowMessageBox(this, 
                                                        $"'{archiveFile.Name}' 파일 압축 해제 중 오류가 발생했습니다.", 
                                                        "압축 풀기 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }));
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    progressForm.Close();
                                    CustomDialogHelper.ShowMessageBox(this, 
                                        $"압축 풀기 중 오류가 발생했습니다: {ex.Message}", 
                                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                            }
                            
                            processedFiles++;
                            progressForm.UpdateFileProgress(processedFiles, totalFiles);
                        }
                        
                        // 작업 완료 후 UI 업데이트
                        this.Invoke(new Action(() =>
                        {
                            // 압축 해제 후 현재 폴더 새로고침
                            m_PreviousFocus.RefreshAll();
                            shellBrowser_Left.RefreshAll();
                            shellBrowser_Right.RefreshAll();
                            
                            progressForm.Close();
                            
                            if (!progressForm.IsCancelled)
                            {
                                CustomDialogHelper.ShowMessageBox(this, 
                                    $"{selectedFiles.Count}개 파일의 압축이 풀렸습니다.", 
                                    "압축 풀기 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }));
                        
                        // 작업 완료 이벤트 호출
                        progressForm.NotifyOperationCompleted();
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            progressForm.Close();
                            CustomDialogHelper.ShowMessageBox(this, 
                                $"압축 풀기 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                CustomDialogHelper.ShowMessageBox(this, 
                    $"압축 풀기 중 오류가 발생했습니다: {ex.Message}", 
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuFileOperations_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                string tag = menuItem.Tag as string;
                switch (tag)
                {
                    case "zip":
                        CompressSelectedFiles();
                        break;
                    case "7zip":
                        CompressSelectedFilesUsing7Zip();
                        break;
                    // ... existing code ...
                }
            }
        }

        /// <summary>
        /// 선택된 파일을 7-Zip을 사용하여 압축
        /// </summary>
        private void CompressSelectedFilesUsing7Zip()
        {
            // 현재 활성화된 셸 브라우저에서 선택된 파일 가져오기
            ShellBrowser activeBrowser = GetActiveBrowser();
            if (activeBrowser == null)
                return;

            // 선택된 항목 가져오기
            string[] selectedItems = activeBrowser.GetSelectedItems();
            if (selectedItems == null || selectedItems.Length == 0)
            {
                MessageBox.Show(
                    StringResources.GetString("NoItemsSelected"),
                    StringResources.GetString("Information"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                // 7-Zip 압축 다이얼로그 표시
                FormSevenZip formSevenZip = new FormSevenZip(selectedItems);
                // 압축 완료 이벤트 등록
                formSevenZip.CompressionCompleted += SevenZip_CompressionCompleted;
                formSevenZip.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"7-Zip 압축 오류: {ex.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 7-Zip 압축 완료 이벤트 처리
        /// </summary>
        private void SevenZip_CompressionCompleted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(SevenZip_CompressionCompleted), sender, e);
                return;
            }
            
            // 양쪽 파일 패널 모두 갱신
            shellBrowser_Left.RefreshAll();
            shellBrowser_Right.RefreshAll();
        }

        /// <summary>
        /// 현재 활성화된 파일 브라우저를 반환합니다.
        /// </summary>
        private ShellBrowser GetActiveBrowser()
        {
            return m_PreviousFocus;
        }

        /// <summary>
        /// 선택된 파일을 ZIP으로 압축합니다.
        /// </summary>
        private void CompressSelectedFiles()
        {
            // 현재 활성화된 셸 브라우저에서 선택된 파일 가져오기
            ShellBrowser activeBrowser = GetActiveBrowser();
            if (activeBrowser == null)
                return;

            // 선택된 항목 가져오기
            string[] selectedItems = activeBrowser.GetSelectedItems();
            if (selectedItems == null || selectedItems.Length == 0)
            {
                MessageBox.Show(
                    StringResources.GetString("NoItemsSelected"),
                    StringResources.GetString("Information"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // 압축 다이얼로그 표시
            GUI.FormPacking formPacking = new GUI.FormPacking(selectedItems);
            formPacking.ShowDialog();

            // 압축 완료 후 파일 목록 새로고침
            activeBrowser.RefreshContents();
        }

        /// <summary>
        /// 컨텍스트 메뉴를 초기화합니다.
        /// </summary>
        private void InitContextMenus()
        {
            // 파일 작업 컨텍스트 메뉴 생성
            ContextMenuFileOperations = new ContextMenuStrip();

            // 파일 메뉴 컨텍스트 메뉴 초기화
            ToolStripMenuItem menuZip = new ToolStripMenuItem(StringResources.GetString("Zip"), null, MenuFileOperations_Click, "zip");
            ToolStripMenuItem menu7Zip = new ToolStripMenuItem("7-Zip", null, MenuFileOperations_Click, "7zip");
            
            ContextMenuFileOperations.Items.Add(menuZip);
            ContextMenuFileOperations.Items.Add(menu7Zip);
        }

        /// <summary>
        /// 작업 완료 시 양쪽 패널 새로 고침
        /// </summary>
        private void ProgressForm_OperationCompleted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(ProgressForm_OperationCompleted), sender, e);
                return;
            }
            
            // 양쪽 파일 패널 모두 갱신
            shellBrowser_Left.RefreshAll();
            shellBrowser_Right.RefreshAll();
        }
    }
    
    /// <summary>
    /// Message filter to intercept form creation and center all forms
    /// </summary>
    public class FormCenteringMessageFilter : IMessageFilter
    {
        private Form _mainForm;
        
        public FormCenteringMessageFilter(Form mainForm)
        {
            _mainForm = mainForm;
        }
        
        public bool PreFilterMessage(ref Message m)
        {
            // WM_SHOWWINDOW message
            if (m.Msg == 0x0018)
            {
                // Try to get the form from the handle
                Control control = Control.FromHandle(m.HWnd);
                if (control is Form form && form != _mainForm && form.Owner == null)
                {
                    // Set owner and center
                    form.Owner = _mainForm;
                    TotalCommander.FormHelper.CenterFormOnParentOrScreen(form);
                }
            }
            return false;
        }
    }
}

