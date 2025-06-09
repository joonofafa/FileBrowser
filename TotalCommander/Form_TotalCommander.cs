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

namespace TotalCommander
{
    public partial class Form_TotalCommander : Form
    {
        #region Fields
        private GUI.ShellBrowser m_PreviousFocus;
        private Font currentAppliedFont;
        private int _tabFocusState = 0;
        private KeySettings keySettings;
        #endregion

        public Form_TotalCommander()
        {
            InitializeComponent();
            
            // Show build date in title bar
            UpdateTitleWithBuildDateTime();
            
            InitializeMenus();
            
            // Load function key settings
            keySettings = KeySettings.Load();
            
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

            // Initialize menus
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
            
            // Space key is handled by file browser, pass it on
            if (keyData == Keys.Space)
            {
                Logger.Debug("ProcessCmdKey: Space key detected, event passed on");
                return false; // Pass event to other control
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
            
            // Tab key handling
            if (keyData == Keys.Tab)
            {
                HandleTabKey();
                return true;
            }
            
            // Handle other keys with default processing
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
                if (ShowDialogCentered(fontDialog) == DialogResult.OK)
                {
                    Font selectedFont = fontDialog.SelectedFont;
                    if (selectedFont != null)
                    {
                        // Apply main font
                        shellBrowser_Left.ApplyFont(selectedFont);
                        shellBrowser_Right.ApplyFont(selectedFont);
                        currentAppliedFont = selectedFont;
                        
                        // Set status bar font
                        bool applyToStatusBar = fontDialog.ApplyToStatusBar;
                        if (applyToStatusBar)
                        {
                            shellBrowser_Left.ApplyStatusBarFont(selectedFont);
                            shellBrowser_Right.ApplyStatusBarFont(selectedFont);
                        }
                        
                        // Save font settings
                        SaveFontSettings(selectedFont, applyToStatusBar);
                    }
                }
            }
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
            // Add menu items to existing menu (mnuMain)
            if (configurationToolStripMenuItem != null)
            {
                // Add User Execute Options menu item (if not already added)
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

                    // Read font settings from XML file
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

                    // Set font style
                    FontStyle style = FontStyle.Regular;
                    if (isBold) style |= FontStyle.Bold;
                    if (isItalic) style |= FontStyle.Italic;
                    if (isUnderline) style |= FontStyle.Underline;
                    if (isStrikeout) style |= FontStyle.Strikeout;

                    // Create and apply font
                    try
                    {
                        currentAppliedFont = new Font(fontFamilyName, fontSize, style);
                        shellBrowser_Left.ApplyFont(currentAppliedFont);
                        shellBrowser_Right.ApplyFont(currentAppliedFont);
                        
                        // Set status bar font
                        if (applyToStatusBar)
                        {
                            shellBrowser_Left.ApplyStatusBarFont(currentAppliedFont);
                            shellBrowser_Right.ApplyStatusBarFont(currentAppliedFont);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Font application error: " + ex.Message);
                        // Use default font
                        currentAppliedFont = SystemFonts.DefaultFont;
                    }
                }
                else
                {
                    // If file doesn't exist, use default font
                    currentAppliedFont = SystemFonts.DefaultFont;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Font settings load error: " + ex.Message);
                // Use default font if error occurs
                currentAppliedFont = SystemFonts.DefaultFont;
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
                    // Refresh destination panel after copy
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

