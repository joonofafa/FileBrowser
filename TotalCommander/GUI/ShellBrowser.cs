using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Runtime.InteropServices;
using TotalCommander;
using System.Collections.Specialized;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace TotalCommander.GUI
{
    public class StatusChangedEventArgs : EventArgs
    {
        public string StatusText { get; }
        public StatusChangedEventArgs(string statusText)
        {
            StatusText = statusText;
        }
    }
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
    public partial class ShellBrowser : UserControl
    {
        #region Static fields
        private static readonly string OwnerOrUser = Environment.UserDomainName + "\\" + Environment.UserName;
        public static ImageList SmallImageList;
        #endregion

        #region Fields
        /// <summary>
        /// <see cref="ShellHistory"/>
        /// </summary>
        private ShellHistory m_History;
        private bool m_HideNavigationPane = false;
        private bool CanCut = false;
        private ListViewItem[] m_ListItemCache = null; //array to cache items for the virtual list
        private int m_FirstItem = 0; //stores the index of the first item in the cache
        public List<FileSystemInfo> m_ShellItemInfo = new List<FileSystemInfo>();
        public string CurrentPath = Path.GetPathRoot(Environment.SystemDirectory);
        private SortOrder Order = SortOrder.None;
        private int SortColumn = 0;
        
        /// <summary>
        /// Whether currently browsing inside an archive file
        /// </summary>
        private bool m_IsInsideArchive = false;
        
        /// <summary>
        /// Path of the currently opened archive file
        /// </summary>
        private string m_CurrentArchivePath = string.Empty;
        
        /// <summary>
        /// Temporary work directory path
        /// </summary>
        private string m_TempWorkDir = string.Empty;
        #endregion

        #region Event hanlder

        public event EventHandler RecvFocus;
        
        public event StatusChangedEventHandler StatusChanged;

        #endregion Event hanlder

        #region Public Properties
        public Label BottomStatusLabel => lblBotStatus;
        
        /// <summary>
        /// File explorer control accessor
        /// </summary>
        public ListView FileExplorer => browser;
        #endregion

        /// <summary>
        /// Displays a message in the status bar.
        /// </summary>
        /// <param name="message">Message to display</param>
        public void SetStatusMessage(string message)
        {
            // Set status message
            if (lblBotStatus != null)
            {
                lblBotStatus.Text = message;
                
                // Trigger StatusChanged event
                StatusChanged?.Invoke(this, new StatusChangedEventArgs(message));
            }
        }

        public ShellBrowser()
        {
            InitializeComponent();
        }

        public void Init()
        {
            /// firstly, assign imagelist to another control
            browser.SmallImageList = SmallImageList;
            navigationPane.ImageList = SmallImageList;

            InitNavigationPane();
            InitBrowser();
            InitHistoryAndPath();
            // open default path
            ProcessFileOrFolderPath(CurrentPath);
            InitDisksBrowser();
            InitTxtPath();
            InitPassingFocus();
            
            // Adjust layout
            AdjustLayout();
            
            // Select first item after initialization
            SelectFirstItem();
        }

        void InitPassingFocus()
        {
            browser.GotFocus += WhenGotFocus;
            txtPath.GotFocus += WhenGotFocus;
            lblBotStatus.GotFocus += WhenGotFocus;
            lblTopStorageStatus.GotFocus += WhenGotFocus;
            navigationPane.GotFocus += WhenGotFocus;
            disksBrowser.GotFocus += WhenGotFocus;
        }

        void WhenGotFocus(object sender, EventArgs e)
        {
            RecvFocus(this, e);
        }

        private void InitHistoryAndPath()
        {
            m_History = new ShellHistory();
        }

        #region Refresh, View Mode, Hide/show directory view

        public bool HideNavigationPane
        {
            get { return m_HideNavigationPane; }
            set
            {
                if (m_HideNavigationPane == value) return;
                m_HideNavigationPane = value;
                if (m_HideNavigationPane)
                {
                    splMainView.Panel1Collapsed = true;
                    splMainView.Panel1.Hide();
                }
                else
                {
                    splMainView.Panel1Collapsed = false;
                    splMainView.Panel1.Show();
                }
            }
        }

        public void ChangeViewMode(View view)
        {
            browser.ChangeViewMode(view);
        }

        public void RefreshAll()
        {
            RefreshListView();
            OnDeviceDetected(null, EventArgs.Empty);
        }

        #endregion Refresh, View Mode

        #region Drive detector: INSERTING OR REMOVING

        public void OnDeviceDetected(object sender, EventArgs e)
        {
            navigationPane.UpdateDisks();
            if (!disksBrowser.UpdateDisks())
            {
                m_History.Clear();
                string path = Path.GetPathRoot(Environment.SystemDirectory);
                ProcessFileOrFolderPath(path);
                disksBrowser.SelectedIndex = 0;
                ComboBoxItem cbi = (ComboBoxItem)disksBrowser.SelectedItem;
                DriveInfo drive = (DriveInfo)cbi.Value;
                SetStorageStatus(drive);
            }
        }

        #endregion Drive detector

        #region Tree View as NavigationPane

        private void InitNavigationPane()
        {
            navigationPane.Init();
            navigationPane.NodeMouseDoubleClick += NavigationPane_NodeMouseDoubleClick;
            navigationPane.KeyDown += TvwNavigationPane_KeyDown;
            
            // Set navigation pane to use full height since path display area is hidden
            navigationPane.Dock = DockStyle.Fill;
        }

        void TvwNavigationPane_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                string path = navigationPane.SelectedNode.Tag.ToString();
                NavigationPane_Click(path);
            }
        }

        private void NavigationPane_Click(string path)
        {
            if (ProcessFolder(path))
            {
                m_History.Add(path);
                RefreshDisksBrowser(CurrentPath, disksBrowser.SelectedItem.ToString());
                
                // Select first item
                SelectFirstItem();
            }
            else if (Directory.Exists(path))
            {
                ProcessFolder(CurrentPath);
            }
        }

        void NavigationPane_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                string path = navigationPane.SelectedNode.Tag.ToString();
                if (string.IsNullOrWhiteSpace(path) || NavigationPane.IsSpecialFolders(path))
                    return;
                NavigationPane_Click(path);
            }
        }

        #endregion Tree View as NavigationPane

        #region Textbox presents Current Path
        private void InitTxtPath()
        {
            // Initialize address bar
            txtPath.Visible = true; // Always show address bar
            txtPath.Text = CurrentPath;
            txtPath.LostFocus += TxtPath_LostFocus;
            txtPath.KeyDown += TxtPath_KeyDown;
            
            // Set address bar height similar to window title bar (about 30px)
            txtPath.Height = 30;
            txtPath.Font = new Font(txtPath.Font.FontFamily, 12, txtPath.Font.Style);
            
            // Set not accessible by Tab key
            txtPath.TabStop = false;
        }

        /// <summary>
        /// Sets focus to the address bar.
        /// </summary>
        public void FocusAddressBar()
        {
            txtPath.Text = CurrentPath;
            txtPath.Focus();
            txtPath.SelectAll();
        }

        void TxtPath_LostFocus(object sender, EventArgs e)
        {
            // Update to current path when focus is lost
                txtPath.Text = CurrentPath;
        }

        void TxtPath_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    string path = txtPath.Text.Trim();
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (Directory.Exists(path))
                        {
                            ProcessFileOrFolderPath(path);
                            m_History.Add(path);
                        }
                        else if (File.Exists(path))
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(path);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(StringResources.GetString("FileOpenError", ex.Message), 
                                               StringResources.GetString("Error"), 
                                               MessageBoxButtons.OK, 
                                               MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show(StringResources.GetString("PathNotFound", path), 
                                           StringResources.GetString("Error"), 
                                           MessageBoxButtons.OK, 
                                           MessageBoxIcon.Error);
                        }
                    }
                    // Set focus to file browser
                    FocusFileBrowser();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Escape:
                    // Move focus to file browser when ESC key is pressed
                    FocusFileBrowser();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
            }
        }
        #endregion Textbox presents Current Path

        #region File browser

        void InitBrowser()
        {
            browser.Init();
            browser.VirtualMode = true;
            browser.VirtualListSize = 0;

            #region Events

            browser.MouseDoubleClick += Browser_MouseDoubleClick;
            browser.MouseUp += Browser_MouseUp;
            browser.KeyDown += Browser_KeyDown;
            browser.AfterLabelEdit += Browser_AfterLabelEdit;
            browser.ColumnClick += Browser_ColumnClick;

            browser.DragEnter += Browser_DragEnter;
            browser.DragDrop += Browser_DragDrop;
            browser.ItemDrag += Browser_ItemDrag;

            browser.CacheVirtualItems += Browser_CacheVirtualItems;
            browser.RetrieveVirtualItem += Browser_RetrieveVirtualItem;
            browser.SearchForVirtualItem += Browser_SearchForVirtualItem;
            
            // Register event handlers
            browser.SelectedIndexChanged -= Browser_SelectedIndexChanged; // Remove previously registered handler if exists
            browser.SelectedIndexChanged += Browser_SelectedIndexChanged;
            
            // Additional events to detect selection changes in virtual mode
            browser.MouseClick -= Browser_MouseClick;
            browser.MouseClick += Browser_MouseClick;
            browser.KeyUp -= Browser_KeyUp;
            browser.KeyUp += Browser_KeyUp;

            #endregion Events
        }

        private void Browser_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (Order == SortOrder.Ascending)
                    Order = SortOrder.Descending;
                else
                    Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                SortColumn = e.Column;
                Order = SortOrder.Ascending;
            }

            // set the sort arrow to a particular column
            browser.SetSortIcon(e.Column, Order);

            // Get the appropriate comparer.
            Comparison<FileSystemInfo> comparer;
            switch (e.Column)
            {
                case 1:
                    comparer = new Comparison<FileSystemInfo>(CompareFileExtension);
                    break;
                case 2:
                    comparer = new Comparison<FileSystemInfo>(CompareFileSize);
                    break;
                case 3:
                    comparer = new Comparison<FileSystemInfo>(CompareFileCreationDate);
                    break;
                case 4:
                    comparer = new Comparison<FileSystemInfo>(CompareFileAttributes);
                    break;
                default: // case 0 or anything else
                    comparer = new Comparison<FileSystemInfo>(CompareFileName);
                    break;
            }

            // Sort the data
            m_ShellItemInfo.Sort((a, b) =>
            {
                int result = comparer(a, b);
                if (Order == SortOrder.Descending)
                {
                    result = -result;
                }
                return result;
            });

            // Invalidate cache and refresh
            m_ListItemCache = null;
            browser.Refresh();
        }

        #region File comparer
        static int CompareFileName(FileSystemInfo a, FileSystemInfo b)
        {
            int result = 0;
            int flag = (a.Attributes.HasFlag(FileAttributes.Directory) ? 1 : 0) +
                (b.Attributes.HasFlag(FileAttributes.Directory) ? 2 : 0);
            switch (flag)
            {
                // Neither item is a folder => Compare names
                case 0: goto case 3;
                // A is a folder, but B isn't => A < B
                case 1: result = -1; break;
                // B is a folder, but A isn't => A > B
                case 2: result = 1; break;
                // Both items are folders => Compare names
                case 3: result = a.Name.CompareTo(b.Name); break;
                // Failsafe
                default:
                    result = 0;
                    break;
            }
            return result;
        }

        static int CompareFileSize(FileSystemInfo a, FileSystemInfo b)
        {
            int result = 0;
            int flag = (a.Attributes.HasFlag(FileAttributes.Directory) ? 1 : 0) +
                (b.Attributes.HasFlag(FileAttributes.Directory) ? 2 : 0);
            switch (flag)
            {
                // Neither item is a folder => Compare sizes
                case 0:
                    var x = (FileInfo)a;
                    var y = (FileInfo)b;
                    result = x.Length.CompareTo(y.Length);
                    break;
                // A is a folder, but B isn't => A < B
                case 1: result = -1; break;
                // B is a folder, but A isn't => A > B
                case 2: result = 1; break;
                // Both items are folders => Not compare
                case 3: result = 0; break;
                // Failsafe
                default:
                    result = 0;
                    break;
            }
            return result;
        }

        static int CompareFileCreationDate(FileSystemInfo a, FileSystemInfo b)
        {
            return a.CreationTime.CompareTo(b.CreationTime);
        }

        static int CompareFileExtension(FileSystemInfo a, FileSystemInfo b)
        {
            return a.Extension.CompareTo(b.Extension);
        }

        static int CompareFileAttributes(FileSystemInfo a, FileSystemInfo b)
        {
            var x = ShellInfoItem.GetFileAttributesString(a.Attributes);
            var y = ShellInfoItem.GetFileAttributesString(b.Attributes);
            return x.CompareTo(y);
        }
        #endregion

        #region Virtual listview hanlder functions
        private void Browser_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {
            var y = m_ShellItemInfo.Skip(e.StartIndex).Where(x => x.Name.StartsWith(e.Text, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (y.Length > 0)
            {
                e.Index = m_ShellItemInfo.IndexOf(y[0]);
            }
        }

        private void Browser_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (m_ListItemCache != null && e.ItemIndex >= m_FirstItem && e.ItemIndex < m_FirstItem + m_ListItemCache.Length)
            {
                e.Item = m_ListItemCache[e.ItemIndex - m_FirstItem];
            }
            else
            {
                e.Item = InitListviewItem(m_ShellItemInfo[e.ItemIndex]);
            }
        }

        private void Browser_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            if (m_ListItemCache != null && e.StartIndex >= m_FirstItem && e.EndIndex <= m_FirstItem + m_ListItemCache.Length)
                return;
            m_FirstItem = e.StartIndex;
            int end = e.EndIndex;
            m_ListItemCache = new ListViewItem[end - m_FirstItem + 1];
            int index = 0;
            for (int i = m_FirstItem; i <= end; i++)
            {
                m_ListItemCache[index++] = InitListviewItem(m_ShellItemInfo[i]);
            }
        }
        #endregion

        #region Other operations such as label edit, drag&drop, keydown
        /// <summary>
        /// Renames selected item by pressing F2 key
        /// </summary>
        void Browser_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                if (string.IsNullOrEmpty(e.Label))
                {
                    e.CancelEdit = true;
                    return;
                }
                string warningMsg = string.Empty;
                try
                {
                    ListViewItem item = browser.Items[e.Item];
                    string source = item.Tag.ToString();
                    FileInfo info = new FileInfo(source);
                    if (info.Attributes.HasFlag(FileAttributes.Directory))
                        FileSystem.RenameDirectory(source, e.Label);
                    else
                        FileSystem.RenameFile(source, e.Label);
                    string newFullName = Path.Combine(CurrentPath, e.Label);
                    item.Tag = newFullName;

                    #region Renames multiple items
                    if (browser.SelectedIndices.Count > 1)
                    {
                        foreach (int i in browser.SelectedIndices)
                        {
                            if (browser.Items[i] != item)
                            {
                                string target = GetDefaultDirectoryName(CurrentPath, e.Label);
                                source = browser.Items[i].Tag.ToString();
                                info = new FileInfo(source);
                                if (info.Attributes.HasFlag(FileAttributes.Directory))
                                    FileSystem.RenameDirectory(source, target);
                                else
                                    FileSystem.RenameFile(source, target);
                                newFullName = Path.Combine(CurrentPath, target);
                            }
                        }
                        RefreshListView();
                    }
                    #endregion Renames multiple items
                }
                catch (ArgumentException)
                {
                    warningMsg = StringResources.GetString("InvalidPathReason", e.Label);
                }
                catch (FileNotFoundException)
                {
                    warningMsg = StringResources.GetString("FileNotFound");
                }
                catch (PathTooLongException)
                {
                    warningMsg = StringResources.GetString("FileNameTooLong");
                }
                catch (IOException) { warningMsg = StringResources.GetString("FileAlreadyExists", e.Label); }
                catch (NotSupportedException) { warningMsg = StringResources.GetString("InvalidName"); }
                catch (UnauthorizedAccessException) { warningMsg = StringResources.GetString("PermissionDenied"); }
                finally
                {
                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        e.CancelEdit = true;
                        MessageBox.Show(warningMsg, StringResources.GetString("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        void Browser_ItemDrag(object sender, ItemDragEventArgs e)
        {
            FileBrowser fBrowser = (FileBrowser)sender;
            List<string> pathList = new List<string>();
            foreach (int i in fBrowser.SelectedIndices)
            {
                pathList.Add(fBrowser.Items[i].Tag.ToString());
            }
            var items = pathList.ToArray();
            DataObject data = new DataObject(DataFormats.FileDrop, items);
            data.SetData(items);
            fBrowser.DoDragDrop(data, DragDropEffects.Copy);
        }

        async void Browser_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop, false)) return;

            string[] dropItems = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (null == dropItems) return;

            bool isSameFolder = Path.GetDirectoryName(dropItems[0]).Equals(CurrentPath, StringComparison.OrdinalIgnoreCase);
            if (isSameFolder)
                return;

            List<Task> taskList = new List<Task>();

            foreach (string source in dropItems)
            {
                string target = Path.GetFileName(source);
                target = Path.Combine(CurrentPath, target);
                Task task = null;
                if (File.Exists(source))
                    task = Task.Run(() => FileSystem.CopyFile(source, target, UIOption.OnlyErrorDialogs, UICancelOption.ThrowException));
                else if (Directory.Exists(source))
                    task = Task.Run(() => FileSystem.CopyDirectory(source, target, UIOption.OnlyErrorDialogs, UICancelOption.ThrowException));
                taskList.Add(task);
            }

            foreach (Task task in taskList)
            {
                try
                {
                    await task;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(StringResources.GetString("CopyError", ex.Message), StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { RefreshListView(); }
            }
        }

        void Browser_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        void Browser_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                FileBrowser fBrowser = (FileBrowser)sender;
                ShellContextMenu menu = new ShellContextMenu();
                if (fBrowser.SelectedIndices.Count > 0)
                {
                    List<FileInfo> fiList = new List<FileInfo>();
                    foreach (int index in fBrowser.SelectedIndices)
                    {
                        FileInfo fInfo = new FileInfo(fBrowser.Items[index].Tag.ToString());
                        fiList.Add(fInfo);
                    }
                    menu.ShowContextMenu(fiList.ToArray(), fBrowser.PointToScreen(e.Location));
                }
                else
                {
                    DirectoryInfo dInfo = new DirectoryInfo(CurrentPath);
                    menu.ShowContextMenu(new DirectoryInfo[] { dInfo }, fBrowser.PointToScreen(e.Location));
                }
            }
        }

        void Browser_KeyDown(object sender, KeyEventArgs e)
        {
            // Add key event logging
            Logger.Debug($"ShellBrowser.Browser_KeyDown: KeyCode={e.KeyCode}, KeyData={e.KeyData}, Handled={e.Handled}");
            
            switch (e.KeyData)
            {
                case Keys.Delete:
                    DeleteSelectedItems(RecycleOption.SendToRecycleBin);
                    break;
                case Keys.Shift | Keys.Delete:
                    DeleteSelectedItems(RecycleOption.DeletePermanently);
                    break;
                case Keys.F2:
                    if (browser.SelectedIndices.Count > 0)
                    {
                        browser.FocusedItem.BeginEdit();
                    }
                    break;
                case Keys.Control | Keys.F:
                    SearchItems();
                    break;
                case Keys.Control | Keys.C:
                    CopySelectedItems();
                    break;
                case Keys.Control | Keys.X:
                    CutSelectedItems();
                    break;
                case Keys.Control | Keys.V:
                    PasteFromClipboard();
                    break;
                case Keys.F5:
                    RefreshListView();
                    break;
                case Keys.Alt | Keys.Enter:
                    OpenPropertiesWindowWithSelectedItems();
                    break;
                case Keys.Alt | Keys.Up:
                    GoParent();
                    break;
                case Keys.Back:
                case Keys.Alt | Keys.Left:
                    GoBackward();
                    break;
                case Keys.Alt | Keys.Right:
                    GoForward();
                    break;
                case Keys.Enter:
                    if (browser.FocusedItem != null)
                    {
                        string path = browser.FocusedItem.Tag.ToString();
                        ProcessFileOrFolderPath(path);
                    }
                    break;
                case Keys.Control | Keys.Shift | Keys.N:
                    CreateNewFolder();
                    break;
                // Space key is handled directly in the FileBrowser class.
            }
        }

        void Browser_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FileBrowser fBrowser = (FileBrowser)sender;
                ListViewHitTestInfo info = fBrowser.HitTest(e.X, e.Y);
                ListViewItem item = info.Item;
                if (item != null)
                {
                    string fullPath = item.Tag.ToString();
                    ProcessFileOrFolderPath(fullPath);
                }
            }
        }

        private void Browser_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Debug message (uncomment if needed)
            // MessageBox.Show("Selection changed!");

            if (browser.SelectedIndices.Count == 0)
            {
                UpdateFolderSummaryStatus();
                return;
            }

            long totalSize = 0;
            int selectedFiles = 0;
            int selectedDirs = 0;

            foreach (int index in browser.SelectedIndices)
            {
                if (index >= 0 && index < m_ShellItemInfo.Count)
                {
                    FileSystemInfo info = m_ShellItemInfo[index];
                    if (info is FileInfo fileInfo)
                    {
                        totalSize += fileInfo.Length;
                        selectedFiles++;
                    }
                    else if (info is DirectoryInfo)
                    {
                        selectedDirs++;
                    }
                }
            }

            string statusText;
            if (selectedFiles > 0 && selectedDirs > 0)
            {
                statusText = StringResources.GetString("SelectedFilesDirs", selectedFiles, selectedDirs, FormatBytes(totalSize));
            }
            else if (selectedFiles > 0)
            {
                statusText = StringResources.GetString("SelectedFiles", selectedFiles, FormatBytes(totalSize));
            }
            else if (selectedDirs > 0)
            {
                statusText = StringResources.GetString("SelectedDirs", selectedDirs);
            }
            else
            {
                UpdateFolderSummaryStatus();
                return;
            }

            // First update the label directly 
            lblBotStatus.Text = statusText;
            
            // Then trigger the event
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(statusText));
        }

        public void UpdateFolderSummaryStatus()
        {
            if (CurrentPath != null && Directory.Exists(CurrentPath))
            {
                try
                {
                    int files = m_ShellItemInfo.Count(f => f is FileInfo);
                    int folders = m_ShellItemInfo.Count(f => f is DirectoryInfo);
                    string statusText = StringResources.GetString("FolderSummary", files, folders);
                    StatusChanged?.Invoke(this, new StatusChangedEventArgs(statusText));
                }
                catch (Exception)
                {
                    string errorText = StringResources.GetString("ErrorReadingFolderContents");
                    StatusChanged?.Invoke(this, new StatusChangedEventArgs(errorText));
                }
            }
            else
            {
                StatusChanged?.Invoke(this, new StatusChangedEventArgs(""));
            }
        }

        public static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (bytes == 0) return "0 " + Suffix[0];
            if (bytes < 0) return "-" + FormatBytes(-bytes);

            int i = 0;
            decimal d = (decimal)bytes;
            while (Math.Round(d / 1024) >= 1)
            {
                d /= 1024;
                i++;
            }
            return string.Format("{0:n1} {1}", d, Suffix[i]);
        }
        #endregion

        #endregion File browser

        #region Disks Browser

        void InitDisksBrowser()
        {
            disksBrowser.Init();
            if (disksBrowser.Items.Count > 0)
            {
                disksBrowser.SelectedIndex = 0;
                ComboBoxItem cbi = (ComboBoxItem)disksBrowser.SelectedItem;
                DriveInfo drive = (DriveInfo)cbi.Value;
                SetStorageStatus(drive);
            }

            disksBrowser.SelectionChangeCommitted += DisksBrowser_SelectionChangeCommitted;
        }

        void SetStorageStatus(DriveInfo drive)
        {
            string volumnName = string.IsNullOrEmpty(drive.VolumeLabel) ? NavigationPane.GetDriveType(drive.DriveType) : drive.VolumeLabel;
            lblTopStorageStatus.Text = String.Format(
                "[{0}] {1} free in {2}",
                volumnName,
                ShellInfoItem.GetBytesReadable(drive.TotalFreeSpace),
                ShellInfoItem.GetBytesReadable(drive.TotalSize));
        }

        void DisksBrowser_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (disksBrowser.Items.Count > 0)
            {
                ComboBoxItem cbi = (ComboBoxItem)disksBrowser.SelectedItem;
                DriveInfo drive = (DriveInfo)cbi.Value;
                SetStorageStatus(drive);
                ProcessFileOrFolderPath(drive.Name);
                
                // Select first item
                SelectFirstItem();
                
                // Synchronize folder tree
                SyncNavigationPaneWithCurrentPath();
            }
        }

        #endregion Disks Browser

        #region Edit actions with Microsoft.VisualBasic.FileIO class

        public void CopySelectedItems()
        {
            // Restrict copy operation inside archive files
            if (m_IsInsideArchive)
            {
                CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                    StringResources.GetString("ArchiveCopyRestriction"), 
                    StringResources.GetString("OperationRestricted"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            if (browser.SelectedIndices.Count > 0)
            {
                string fileName = string.Empty;
                var pathCollection = new System.Collections.Specialized.StringCollection();
                foreach (int i in browser.SelectedIndices)
                {
                    fileName = browser.Items[i].Tag.ToString();
                    pathCollection.Add(fileName);
                }
                if (pathCollection.Count > 0)
                {
                    CanCut = false;
                    Clipboard.SetFileDropList(pathCollection);
                }
            }
        }

        public async void DeleteSelectedItems(RecycleOption recycle)
        {
            // Restrict delete operation inside archive files
            if (m_IsInsideArchive)
            {
                CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                    StringResources.GetString("ArchiveDeleteRestriction"), 
                    StringResources.GetString("OperationRestricted"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // Get selected items
            List<FileSystemInfo> itemsToDelete = new List<FileSystemInfo>();
            foreach (int index in browser.SelectedIndices)
            {
                if (index >= 0 && index < m_ShellItemInfo.Count)
                {
                    itemsToDelete.Add(m_ShellItemInfo[index]);
                }
            }
            
            try
            {
                if (itemsToDelete.Count > 0)
                {
                    // Create delete confirmation message
                    string confirmMessage = itemsToDelete.Count == 1 
                        ? StringResources.GetString("DeleteFile", itemsToDelete[0].Name)
                        : StringResources.GetString("DeleteMultiple", itemsToDelete.Count);
                    
                    // CustomDialogHelper를 사용하여 중앙에 표시되는 확인 대화 상자 표시
                    DialogResult result = CustomDialogHelper.ShowMessageBox(
                        this.FindForm(),
                        confirmMessage,
                        StringResources.GetString("DeleteConfirmTitle"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // 삭제 진행 상황을 표시할 커스텀 대화 상자 생성
                    Form mainForm = this.FindForm();
                    using (var progressForm = new Form())
                    {
                        progressForm.Text = StringResources.GetString("DeletingFiles");
                        progressForm.StartPosition = FormStartPosition.Manual;
                        progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        progressForm.MaximizeBox = false;
                        progressForm.MinimizeBox = false;
                        progressForm.Size = new Size(400, 120);
                        progressForm.ShowInTaskbar = false;
                        
                        // Add progress bar
                        ProgressBar progressBar = new ProgressBar();
                        progressBar.Dock = DockStyle.None;
                        progressBar.Location = new Point(20, 20);
                        progressBar.Size = new Size(360, 23);
                        progressBar.Style = ProgressBarStyle.Marquee;
                        progressForm.Controls.Add(progressBar);
                        
                        // Add status label
                        Label statusLabel = new Label();
                        statusLabel.AutoSize = true;
                        statusLabel.Location = new Point(20, 50);
                        statusLabel.Text = StringResources.GetString("DeletingItemsProgress");
                        progressForm.Controls.Add(statusLabel);
                        
                        // 폼 중앙에 배치
                        if (mainForm != null)
                        {
                            FormHelper.CenterFormOnParentOrScreen(progressForm);
                            progressForm.Owner = mainForm;
                        }
                        
                        // 비동기 작업 시작
                        var deleteTask = Task.Run(() => 
                        {
                            List<Task> taskList = new List<Task>();
                            foreach (FileSystemInfo itemInfo in itemsToDelete)
                            {
                                string fileName = itemInfo.FullName;
                                
                                Task task = Task.Run(() => 
                                {
                                    try
                                    {
                            if (File.Exists(fileName))
                                        {
                                            if (recycle == RecycleOption.SendToRecycleBin)
                                                FileSystem.DeleteFile(fileName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                                            else
                                                File.Delete(fileName);
                                        }
                            else if (Directory.Exists(fileName))
                                        {
                                            if (recycle == RecycleOption.SendToRecycleBin)
                                                FileSystem.DeleteDirectory(fileName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                                            else
                                                Directory.Delete(fileName, true);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        progressForm.Invoke(new Action(() => 
                                        {
                                            statusLabel.Text = StringResources.GetString("DeletionError", ex.Message);
                                        }));
                                    }
                                });
                                
                                taskList.Add(task);
                            }
                            
                            try
                            {
                                Task.WaitAll(taskList.ToArray());
                            }
                            catch (Exception ex)
                            {
                                progressForm.Invoke(new Action(() => 
                                {
                                    statusLabel.Text = StringResources.GetString("DeletionError", ex.Message);
                                }));
                            }
                            
                            // 작업 완료 후 폼 닫기
                            progressForm.Invoke(new Action(() => 
                            {
                                progressForm.Close();
                            }));
                        });
                        
                        // 대화 상자 표시 및 삭제 작업 완료 대기
                        await Task.Run(() => progressForm.ShowDialog());
                    }
                }
            }
            finally
            {
                // Refresh the list after operation completes
                RefreshListView();
            }
        }

        public void CutSelectedItems()
        {
            // Restrict cut operation inside archive files
            if (m_IsInsideArchive)
            {
                CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                    StringResources.GetString("ArchiveCutRestriction"), 
                    StringResources.GetString("OperationRestricted"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            CopySelectedItems();
            CanCut = true;
        }

        #region Delegates
        private delegate void PasteFileDel(string source, string dest, UIOption uiOption, UICancelOption cancelOption);
        private delegate void PasteDirDel(string source, string dest, UIOption uiOption, UICancelOption cancelOption);
        #endregion Delegates
        
        /// <summary>
        /// Paste selected items from clipboard
        /// </summary>
        public void PasteFromClipboard()
        {
            // Restrict paste operation inside archive files
            if (m_IsInsideArchive)
            {
                CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                    StringResources.GetString("ArchivePasteRestriction"), 
                    StringResources.GetString("OperationRestricted"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            if (Clipboard.ContainsFileDropList())
            {
                string[] files = Clipboard.GetFileDropList().Cast<string>().ToArray();
                if (files.Length > 0)
                {
                    CanCut = false;
                    string destPath = CurrentPath;
                    var copyProcessor = new FormProgressCopy(files, destPath, CanCut);
                    // 대화 상자를 표시하지 않고 백그라운드에서 작업 실행
                    copyProcessor.ProcessFilesInBackground();
                    RefreshListView();
                }
            }
        }

        private void RefreshListView()
        {
            // Pause screen updates
            browser.BeginUpdate();
            
            try
            {
                // Initialize cache
                m_ListItemCache = null;
                m_FirstItem = 0;
                
                // Reprocess current path (refresh list)
                ProcessFolder(CurrentPath);
                
                // Reset scroll position
                browser.TopItem = null;
            }
            finally
            {
                // Resume screen updates
                browser.EndUpdate();
            }
            
            // Force screen refresh
            browser.Invalidate(true);
            browser.Update();
            
            // Select first item after refreshing the list
            SelectFirstItem();
            
            // Synchronize folder tree
            SyncNavigationPaneWithCurrentPath();
        }

        private void RefreshTreeView()
        {
            navigationPane.Refresh();
        }

        /// <summary>
        /// Open properties window of selected items
        /// </summary>
        public void OpenPropertiesWindowWithSelectedItems()
        {
            if (browser.SelectedIndices.Count == 0)
                return;

            try
            {
                List<string> fileNames = new List<string>();
                foreach (int index in browser.SelectedIndices)
                {
                    FileSystemInfo info = m_ShellItemInfo[index];
                    fileNames.Add(info.FullName);
                }
                if (fileNames.Count > 0)
                {
                    ShellProperties.ShowFileProperties(fileNames.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PackFiles()
        {
            var selectedIndices = browser.SelectedIndices;
            if (selectedIndices.Count > 0)
            {
                string[] arrPaths = new string[selectedIndices.Count];
                int count = 0;
                foreach (int item in selectedIndices)
                {
                    arrPaths[count++] = browser.Items[item].Tag.ToString();
                }
                using (var frmPacking = new FormPacking(arrPaths))
                {
                    if (!frmPacking.IsDisposed)
                    {
                        var result = ShowDialogCentered(frmPacking);
                        if (result == DialogResult.OK)
                            RefreshListView();
                    }
                }
            }
        }

        static string NotepadLocation = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows), "notepad.exe");
        /// <summary>
        /// Opens notepad to edit selected items
        /// </summary>
        public void EditWithNotepad()
        {
            if (browser.SelectedIndices.Count > 0)
            {
                int index = browser.SelectedIndices[0];
                FileSystemInfo info = m_ShellItemInfo[index];
                if (info is FileInfo)
                {
                    Process.Start(NotepadLocation, "\"" + info.FullName + "\"");
                }
            }
        }

        #region Searchs files and folders with pattern
        void SearchItems()
        {
            FormFindFiles frmFind = new FormFindFiles(CurrentPath, browser.SmallImageList);
            ShowDialogCentered(frmFind);
        }
        #endregion Searchs files and folders with pattern

        #region Create New Folder and New File

        public void CreateNewFolder()
        {
            // Restrict folder creation inside archive files
            if (m_IsInsideArchive)
            {
                CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                    StringResources.GetString("ArchiveFolderCreateRestriction"), 
                    StringResources.GetString("OperationRestricted"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            string targetPath = CurrentPath;
            
            // Check if we can create a new folder at the current location
            if (!Directory.Exists(targetPath))
            {
                MessageBox.Show(StringResources.GetString("CannotCreateFolder"),
                    StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Create new folder dialog
            using (FormNewFolder folderDialog = new FormNewFolder())
            {
                folderDialog.Text = StringResources.GetString("NewFolder");
                folderDialog.NewName = GetDefaultDirectoryName(targetPath);
                folderDialog.Init();
                
                // Show dialog
                if (ShowDialogCentered(folderDialog) == DialogResult.OK)
                {
                    string newName = folderDialog.NewName;
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        MessageBox.Show(StringResources.GetString("InvalidFolderName"),
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    // 새 폴더 경로 생성
                    string newFolderPath = Path.Combine(targetPath, newName);
                    
                    // 같은 이름의 폴더가 이미 존재하는지 확인
                    if (Directory.Exists(newFolderPath))
                    {
                        MessageBox.Show(StringResources.GetString("FolderExists"),
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    // 같은 이름의 파일이 이미 존재하는지 확인
                    if (File.Exists(newFolderPath))
                    {
                        MessageBox.Show(StringResources.GetString("FileExists"),
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    try
                    {
                        // 폴더 생성
                        Directory.CreateDirectory(newFolderPath);
                        
                        // 목록 새로 고침
                        RefreshListView();
                        
                        // 새로 생성된 폴더를 선택
                        SelectCreatedFolder(newFolderPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(StringResources.GetString("CreateFolderFailed") + "\n" + ex.Message,
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        public void CreateNewFile()
        {
            // Restrict file creation inside archive files
            if (m_IsInsideArchive)
            {
                CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                    StringResources.GetString("ArchiveFileCreateRestriction"), 
                    StringResources.GetString("OperationRestricted"), 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            string targetPath = CurrentPath;
            
            // Check if we can create a new file at the current location
            if (!Directory.Exists(targetPath))
            {
                MessageBox.Show(StringResources.GetString("CannotCreateFile"),
                    StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Create new file dialog
            using (FormNewFolder fileDialog = new FormNewFolder())
            {
                fileDialog.Text = StringResources.GetString("NewFile");
                fileDialog.NewName = "New Text Document.txt";
                fileDialog.Init();
                
                // Show dialog
                if (ShowDialogCentered(fileDialog) == DialogResult.OK)
                {
                    string newName = fileDialog.NewName;
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        MessageBox.Show(StringResources.GetString("InvalidFileName"),
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    // 새 파일 경로 생성
                    string newFilePath = Path.Combine(targetPath, newName);
                    
                    // 같은 이름의 파일이 이미 존재하는지 확인
                    if (File.Exists(newFilePath))
                    {
                        MessageBox.Show(StringResources.GetString("FileExists"),
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    // 같은 이름의 폴더가 이미 존재하는지 확인
                    if (Directory.Exists(newFilePath))
                    {
                        MessageBox.Show(StringResources.GetString("FolderExists"),
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    try
                    {
                        // 빈 파일 생성
                        using (File.Create(newFilePath)) { }
                        
                        // 목록 새로 고침
                        RefreshListView();
                        
                        // 새로 생성된 파일을 선택
                        SelectCreatedFile(newFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(StringResources.GetString("CreateFileFailed") + "\n" + ex.Message,
                            StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        static string GetDefaultDirectoryName(string currentPath, string newName = "New Folder")
        {
            string fullname = Path.Combine(currentPath, newName);
            int suffix = 0;
            while (Directory.Exists(fullname) || File.Exists(fullname))
            {
                fullname = string.Format("{0}\\{1} ({2})",
                    currentPath, newName, ++suffix);
            }
            return Path.GetFileName(fullname);
        }

        #endregion CreateNewFolder

        #endregion Edit actions with Microsoft.VisualBasic.FileIO class

        #region Process Files or Folders

        public static DirectoryInfo[] GetSubDirectories(DirectoryInfo path)
        {
            DirectoryInfo[] subDirs = null;
            try
            {
                SetAccessFolderRule(path.FullName);
                subDirs = path.GetDirectories().Where(x => !NavigationPane.BannedAttrExists(x)).ToArray();
            }
            catch (System.UnauthorizedAccessException)
            {
                return null;
            }
            return subDirs;
        }

        public static FileInfo[] GetSubFiles(DirectoryInfo path)
        {
            FileInfo[] subFiles = null;
            try
            {
                SetAccessFolderRule(path.FullName);
                subFiles = path.GetFiles().Where(x => !NavigationPane.BannedAttrExists(x)).ToArray();
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show(StringResources.GetString("AccessDeniedOnFile", path.FullName),
                    StringResources.GetString("TotalCommander"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return subFiles;
        }

        /// <summary>
        /// Process folder path, and gets result indicating that it is a folder,
        /// and we have permissions on this folder
        /// </summary>
        private bool ProcessFolder(string path)
        {
            if (Directory.Exists(path))
            {
                if (Navigate(path))
                {
                    txtPath.Text = CurrentPath; // Update address bar
                    UpdateFolderSummaryStatus();
                    return true;
                }
            }
            return false;
        }

        private void ProcessFileOrFolderPath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return;

                // 먼저 디렉토리 처리 시도
                if (Directory.Exists(path))
                {
                    // 탐색 시 압축 파일 내부 상태 해제
                    if (m_IsInsideArchive)
                    {
                        // 임시 디렉토리 정리
                        CleanupArchiveView();
                    }
                    
                    // 폴더 경로 처리
                    if (Navigate(path))
                    {
                        RefreshDisksBrowser(path, CurrentPath);
                        SyncNavigationPaneWithCurrentPath();
                    }
                    return;
                }

                // 파일 처리
                if (File.Exists(path))
                {
                    FileInfo fileInfo = new FileInfo(path);
                    
                    // 압축 파일인 경우
                    if (IsArchiveFile(path))
                    {
                        // 기존 압축 파일 내부 상태 해제
                        if (m_IsInsideArchive)
                        {
                            // 임시 디렉토리 정리
                            CleanupArchiveView();
                        }
                        
                        // 압축 파일 내용 처리
                        ProcessArchiveFile(path);
                        return;
                    }
                    
                    // 실행 가능한 파일인 경우
                Process.Start(path);
                    return;
                }
                
                SetStatusMessage($"경로를 찾을 수 없습니다: {path}");
            }
            catch (Exception ex)
            {
                SetStatusMessage($"오류: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clean up temporary files created for archive browsing
        /// </summary>
        private void CleanupArchiveView()
        {
            try
            {
                m_IsInsideArchive = false;
                m_CurrentArchivePath = string.Empty;
                
                // 임시 디렉토리 삭제
                if (!string.IsNullOrEmpty(m_TempWorkDir) && Directory.Exists(m_TempWorkDir))
                {
                    try
                    {
                        Directory.Delete(m_TempWorkDir, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Failed to delete temporary directory: {m_TempWorkDir}");
                    }
                }
                
                m_TempWorkDir = string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error cleaning up archive view");
            }
        }

        #endregion Process Files or Folders

        #region Navigations

        /// <summary>
        /// Do not check if path is exists
        /// </summary>
        /// <returns>Returns values indicated that we have opened the path</returns>
        private bool Navigate(string path, bool updateHistory = true)
        {
            var currentDir = new DirectoryInfo(path);
            var subDirs = GetSubDirectories(currentDir);
            if (null == subDirs)
            {
                MessageBox.Show(StringResources.GetString("AccessDeniedOnFolder", path),
                    StringResources.GetString("TotalCommander"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            CurrentPath = path;
            // Update address bar
            txtPath.Text = CurrentPath;
            
            // Pause screen updates
            browser.BeginUpdate();
            
            try
            {
                // Initialize cache and data
            m_ListItemCache = null;
                m_FirstItem = 0;
            m_ShellItemInfo.Clear();
                
                // Set virtual list size to 0 to remove all previous items
                browser.VirtualListSize = 0;
                
                // Add subdirectories and files
            var subFiles = GetSubFiles(currentDir);
            if (subDirs.Length > 0)
                m_ShellItemInfo.AddRange(subDirs);
                if (subFiles != null && subFiles.Length > 0)
                m_ShellItemInfo.AddRange(subFiles);
                
                // Reset scroll position
                browser.TopItem = null;
                
                // Set new virtual list size
            browser.VirtualListSize = m_ShellItemInfo.Count;
                
                // Select first item (before resuming screen updates)
                if (browser.VirtualListSize > 0)
                {
                    browser.SelectedIndices.Clear();
                    browser.SelectedIndices.Add(0);
                }
            }
            catch (IOException ex)
            {
                Logger.Error(ex, $"IO Exception when trying to navigate to {path}");
                CustomDialogHelper.ShowMessageBox(this.FindForm(), StringResources.GetString("FileOpenError", ex.Message),
                    StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Generic Exception when trying to navigate to {path}");
                CustomDialogHelper.ShowMessageBox(this.FindForm(), StringResources.GetString("PathNotFound", path),
                    StringResources.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                // Resume screen updates
                browser.EndUpdate();
            }
            
            // Force screen refresh
            browser.Invalidate(true);
            browser.Update();
            
            // Scroll to make first item visible
            if (browser.VirtualListSize > 0)
            {
                browser.EnsureVisible(0);
                browser.FocusedItem = browser.Items[0];
            }
            
            // Synchronize folder tree with current path
            SyncNavigationPaneWithCurrentPath();
            
            return true;
        }
        
        /// <summary>
        /// Synchronizes the selected node in the folder tree (NavigationPane) with the current path.
        /// </summary>
        private void SyncNavigationPaneWithCurrentPath()
        {
            if (!m_HideNavigationPane && !string.IsNullOrEmpty(CurrentPath))
            {
                // Reflect current path in NavigationPane
                navigationPane.SelectNodeByPath(CurrentPath);
            }
        }
        
        /// <summary>
        /// Selects the first item in the list.
        /// </summary>
        private void SelectFirstItem()
        {
            if (browser.VirtualListSize <= 0)
                return;
                
            // Pause screen updates
            browser.BeginUpdate();
            
            try
            {
                // Clear current selection
                browser.SelectedIndices.Clear();
                
                // Reset scroll position
                browser.TopItem = null;
                
                // Select first item
                browser.SelectedIndices.Add(0);
                
                // Set focus
                if (browser.Items.Count > 0)
                    browser.FocusedItem = browser.Items[0];
            }
            finally
            {
                // Resume screen updates
                browser.EndUpdate();
            }
            
            // Force screen refresh
            browser.Invalidate(true);
            browser.Update();
            
            // Scroll to ensure first item is visible
            browser.EnsureVisible(0);
            
            // Give ListView time to process
            Application.DoEvents();
        }

        private static ListViewItem InitListviewItem(FileSystemInfo info)
        {
            var list = SmallImageList;
            var item = new ShellInfoItem(info);
            string[] row = item.ToArray();
            int key = list.Images.IndexOfKey("unknown");
            if (info.Attributes.HasFlag(FileAttributes.Directory))
            {
                var dir = (DirectoryInfo)info;
                try
                {
                    dir.GetAccessControl();
                    if (dir.Attributes.HasFlag(FileAttributes.Hidden))
                        key = list.Images.IndexOfKey("hidden_folder");
                    else
                        key = list.Images.IndexOfKey("FolderIcon");
                }
                catch (UnauthorizedAccessException)
                {
                    //key = list.Images.IndexOfKey("locked_folder");
                    key = list.Images.IndexOfKey("LockFolder");
                }
            }
            else
            {
                var icon = ShellIcon.GetIcon(info.FullName);
                string ext = item.Ext;
                if (icon != null)
                {
                    AddKeyToImageList(info.FullName, icon);
                    key = list.Images.IndexOfKey(info.FullName);
                }
                else if (!String.IsNullOrEmpty(ext))
                {
                    icon = ShellIcon.GetSmallIconFromExtension(ext);
                    AddKeyToImageList(ext, icon);
                    key = list.Images.IndexOfKey(ext);
                }
            }
            var lvi = new ListViewItem(row, key) { Tag = info.FullName };
            return lvi;
        }

        static void AddKeyToImageList(string key, Icon icon)
        {
            if (!SmallImageList.Images.ContainsKey(key))
            {
                SmallImageList.Images.Add(key, icon);
            }
        }

        /// <summary>
        /// Syncs file browser and disk browser
        /// </summary>
        private void RefreshDisksBrowser(string path, string oldPath)
        {
            if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(oldPath))
                return;
            string pathroot = Path.GetPathRoot(path);
            if (!pathroot.Equals(Path.GetPathRoot(oldPath), StringComparison.OrdinalIgnoreCase))
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                int len = allDrives.Length;
                for (int i = 0; i < len; i++)
                {
                    if (allDrives[i].Name.Equals(pathroot, StringComparison.OrdinalIgnoreCase))
                    {
                        disksBrowser.SelectedIndex = i;
                        SetStorageStatus(allDrives[i]);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 뒤로 가기
        /// </summary>
        public void GoBackward()
        {
            string path = m_History.Backward();
            if (!string.IsNullOrEmpty(path))
            {
                // 압축 파일 내부 상태 해제
                if (m_IsInsideArchive)
                {
                    CleanupArchiveView();
                }
                
                if (ProcessFolder(path))
                {
                    RefreshDisksBrowser(CurrentPath, disksBrowser.SelectedItem.ToString());
                    
                    // Select first item
                    SelectFirstItem();
                    
                    // Synchronize folder tree
                    SyncNavigationPaneWithCurrentPath();
                }
            }
        }

        /// <summary>
        /// 앞으로 가기
        /// </summary>
        public void GoForward()
        {
            string path = m_History.Forward();
            if (!string.IsNullOrEmpty(path))
            {
                // 압축 파일 내부 상태 해제
                if (m_IsInsideArchive)
                {
                    CleanupArchiveView();
                }
                
                if (ProcessFolder(path))
                {
                    RefreshDisksBrowser(CurrentPath, disksBrowser.SelectedItem.ToString());
                    
                    // Select first item
                    SelectFirstItem();
                    
                    // Synchronize folder tree
                    SyncNavigationPaneWithCurrentPath();
                }
            }
        }

        /// <summary>
        /// 상위 폴더로 이동
        /// </summary>
        public void GoParent()
        {
            // 압축 파일 내부인 경우 압축 파일 경로로 이동
            if (m_IsInsideArchive && !string.IsNullOrEmpty(m_CurrentArchivePath))
            {
                string parentPath = Path.GetDirectoryName(m_CurrentArchivePath);
                if (!string.IsNullOrEmpty(parentPath))
                {
                    CleanupArchiveView();
                    ProcessFileOrFolderPath(parentPath);
                    m_History.Add(parentPath);
                }
                return;
            }
            
            // 일반 폴더에서 상위 폴더로 이동
            string dirParentPath = Directory.GetParent(CurrentPath)?.FullName;
            if (!string.IsNullOrEmpty(dirParentPath))
            {
                ProcessFileOrFolderPath(dirParentPath);
                m_History.Add(dirParentPath);
                
                // Select first item
                SelectFirstItem();
                
                // Synchronize folder tree
                SyncNavigationPaneWithCurrentPath();
            }
        }

        private int FindItemWithPath(string tag)
        {
            var y = m_ShellItemInfo.Where(x => x.FullName.Equals(tag, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (y.Length > 0)
            {
                return m_ShellItemInfo.IndexOf(y[0]);
            }
            return -1;
        }

        #endregion Navigations

        #region Check & Set folders permission
        private static bool CanAccessFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder.
                // This will raise an exception if the path is read only or do not have access to view the permissions.
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static void SetAccessFolderRule(string directoryPath)
        {
            System.Security.AccessControl.DirectorySecurity sec = System.IO.Directory.GetAccessControl(directoryPath);
            //string owner = sec.GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
            System.Security.AccessControl.FileSystemAccessRule accRule =
                new System.Security.AccessControl.FileSystemAccessRule(
                    OwnerOrUser,
                    System.Security.AccessControl.FileSystemRights.FullControl,
                    System.Security.AccessControl.AccessControlType.Allow);
            sec.AddAccessRule(accRule);
        }
        #endregion Check & Set folders permission

        public void ApplyFont(Font font)
        {
            if (font == null) return;

            // Change font for file list ListView
            if (browser != null)
            {
                browser.Font = font;
            }

            // Change font for folder tree NavigationPane
            if (navigationPane != null)
            {
                navigationPane.Font = font;
            }
            // Change path display TextBox font (optional)
            if (txtPath != null)
            {
                txtPath.Font = font;
            }
        }

        /// <summary>
        /// Sets the font for the bottom status bar.
        /// </summary>
        /// <param name="font">Font to apply</param>
        public void ApplyStatusBarFont(Font font)
        {
            if (font == null || lblBotStatus == null) return;
            
            // Change the status bar font
            // Set it smaller than the main font to save space
            float newSize = Math.Max(font.Size - 2, 7); // Minimum 7pt, 2pt smaller than main font
            Font statusFont = new Font(font.FontFamily, newSize, font.Style);
            lblBotStatus.Font = statusFont;
            
            // Adjust status bar height to match font size
            AdjustStatusBarHeight(statusFont);
        }
        
        /// <summary>
        /// Adjusts the status bar height to match the font size.
        /// </summary>
        /// <param name="font">Font applied to the status bar</param>
        private void AdjustStatusBarHeight(Font font)
        {
            if (lblBotStatus == null || font == null) return;
            
            // Default padding (top and bottom)
            const int padding = 3;
            
            // Calculate new height based on font size - adjust smaller
            int newHeight = (int)Math.Ceiling(font.Height * 1.0) + padding;
            
            // Minimum height restriction - set small
            newHeight = Math.Max(newHeight, 16);
            
            // Only adjust if height is different from current
            if (lblBotStatus.Height != newHeight)
            {
                // Adjust status bar size and position
                lblBotStatus.Height = newHeight;
                lblBotStatus.Location = new System.Drawing.Point(0, this.Height - newHeight);
                
                // Update control layout
                this.PerformLayout();
                
                // Adjust spacing between status bar and browser panel
                if (splMainView != null)
                {
                    // Resize splMainView (fit above status bar)
                    splMainView.Height = this.Height - newHeight - splMainView.Location.Y;
                }
                
                // Apply changes to screen
                lblBotStatus.Invalidate();
                this.Invalidate();
            }
        }

        public void FocusNavigationPane()
        {
            // Multiple approaches are used as Focus() method may not be sufficient
            if (this.navigationPane != null)
            {
                // 1. First try setting focus using custom approach
                this.navigationPane.Select();
                this.navigationPane.Focus();
                
                // 2. Verify if focus was set (for debugging)
                // MessageBox.Show("Navigation panel focus: " + (this.navigationPane.Focused ? "success" : "failure"));
            }
        }

        public void FocusFileBrowser()
        {
            // Multiple approaches are used as Focus() method may not be sufficient
            if (this.browser != null)
            {
                // 1. First try setting focus using custom approach
                this.browser.Select();
                this.browser.Focus();
                
                // 2. Verify if focus was set (for debugging)
                // MessageBox.Show("File browser focus: " + (this.browser.Focused ? "success" : "failure"));
            }
        }
        
        /// <summary>
        /// Gets the current column width settings.
        /// </summary>
        /// <returns>Dictionary containing column index and width</returns>
        public Dictionary<int, int> GetColumnWidths()
        {
            if (browser != null && browser.View == View.Details)
            {
                return browser.GetColumnWidths();
            }
            return new Dictionary<int, int>();
        }
        
        /// <summary>
        /// Applies saved column width settings.
        /// </summary>
        /// <param name="columnWidths">Column width settings to apply</param>
        public void ApplyColumnWidths(Dictionary<int, int> columnWidths)
        {
            if (browser != null && browser.View == View.Details && columnWidths != null)
            {
                browser.ApplyColumnWidths(columnWidths);
            }
        }

        // Additional: Detect selection changes through mouse click events
        private void Browser_MouseClick(object sender, MouseEventArgs e)
        {
            // When click event occurs, selection may have changed, so manually call the SelectedIndexChanged handler
            Browser_SelectedIndexChanged(sender, EventArgs.Empty);
        }
        
        // Additional: Detect selection changes through keyboard events
        private void Browser_KeyUp(object sender, KeyEventArgs e)
        {
            // When keys that can affect selection are pressed (arrow keys, Space, Enter, etc.)
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || 
                e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Home || e.KeyCode == Keys.End ||
                e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            {
                // Selection may have changed, so manually call the SelectedIndexChanged handler
                Browser_SelectedIndexChanged(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Adjusts the layout of controls.
        /// </summary>
        private void AdjustLayout()
        {
            // Set address bar position and size
            txtPath.Location = new System.Drawing.Point(0, 0);
            txtPath.Anchor = System.Windows.Forms.AnchorStyles.Top | 
                            System.Windows.Forms.AnchorStyles.Left | 
                            System.Windows.Forms.AnchorStyles.Right;
            txtPath.Width = this.Width;
            
            // Set address bar height similar to window title bar (about 30px)
            txtPath.Height = 30;
            txtPath.Font = new Font(txtPath.Font.FontFamily, 12, txtPath.Font.Style);
            
            // 주소표시줄 높이를 윈도우 타이틀바와 비슷하게 설정 (약 30px)
            txtPath.Height = 30;
            txtPath.Font = new Font(txtPath.Font.FontFamily, 12, txtPath.Font.Style);
            
            // Hide disk browser and storage status
            disksBrowser.Visible = false;
            lblTopStorageStatus.Visible = false;
            
            // Adjust splMainView position and size
            int topMargin = txtPath.Height + 2; // Small margin below address bar
            splMainView.Location = new System.Drawing.Point(0, topMargin);
            splMainView.Height = this.Height - topMargin - lblBotStatus.Height;
            
            // Update address bar text
            txtPath.Text = CurrentPath;
        }
        
        /// <summary>
        /// Readjusts layout when control size changes.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustLayout();
        }

        /// <summary>
        /// Register column width change event
        /// </summary>
        /// <param name="handler">Column width change event handler</param>
        public void RegisterColumnWidthChangedEvent(EventHandler handler)
        {
            if (browser != null && handler != null)
            {
                browser.ColumnWidthChanged += handler;
            }
        }

        /// <summary>
        /// Unregister column width change event
        /// </summary>
        /// <param name="handler">Column width change event handler</param>
        public void UnregisterColumnWidthChangedEvent(EventHandler handler)
        {
            if (browser != null && handler != null)
            {
                browser.ColumnWidthChanged -= handler;
            }
        }

        /// <summary>
        /// Gets the SplitterDistance value.
        /// </summary>
        /// <returns>Current SplitterDistance value</returns>
        public int GetSplitterDistance()
        {
            if (splMainView != null)
            {
                return splMainView.SplitterDistance;
            }
            return 0;
        }
        
        /// <summary>
        /// Sets the SplitterDistance value.
        /// </summary>
        /// <param name="distance">SplitterDistance value to set</param>
        public void SetSplitterDistance(int distance)
        {
            if (splMainView != null && distance > 0)
            {
                // Limit between minimum and maximum values
                int minDistance = splMainView.Panel1MinSize;
                int maxDistance = splMainView.Width - splMainView.Panel2MinSize - splMainView.SplitterWidth;
                
                // Adjust to valid range
                distance = Math.Max(minDistance, Math.Min(distance, maxDistance));
                
                // Set SplitterDistance
                splMainView.SplitterDistance = distance;
            }
        }
        
        /// <summary>
        /// Subscribe to SplitterDistance change event.
        /// </summary>
        /// <param name="handler">Event handler</param>
        public void RegisterSplitterDistanceChangedEvent(SplitterEventHandler handler)
        {
            if (splMainView != null && handler != null)
            {
                splMainView.SplitterMoved += handler;
            }
        }
        
        /// <summary>
        /// Unsubscribe from SplitterDistance change event.
        /// </summary>
        /// <param name="handler">Event handler</param>
        public void UnregisterSplitterDistanceChangedEvent(SplitterEventHandler handler)
        {
            if (splMainView != null && handler != null)
            {
                splMainView.SplitterMoved -= handler;
            }
        }

        /// <summary>
        /// Renames the selected file or folder.
        /// </summary>
        public void RenameSelectedItem()
        {
            Logger.Debug("ShellBrowser.RenameSelectedItem: Starting rename operation");
            
            try
            {
                // 1. Check if a single item is selected
                if (browser.SelectedIndices.Count != 1)
                {
                    Logger.Debug("ShellBrowser.RenameSelectedItem: Multiple items selected or no items selected");
                    MessageBox.Show(StringResources.GetString("RenameMultipleNotAllowed"), 
                                   StringResources.GetString("RenameTitle"), 
                                   MessageBoxButtons.OK, 
                                   MessageBoxIcon.Information);
                    return;
                }

                // 2. Check if the selected index is valid
                int selectedIdx = browser.SelectedIndices[0];
                if (selectedIdx < 0 || selectedIdx >= m_ShellItemInfo.Count)
                {
                    Logger.Debug($"ShellBrowser.RenameSelectedItem: Invalid selected index {selectedIdx}");
                    return;
                }
                
                // 3. Check if the item is editable (not read-only)
                FileSystemInfo fileInfo = m_ShellItemInfo[selectedIdx];
                if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    Logger.Debug("ShellBrowser.RenameSelectedItem: Selected item is read-only");
                    MessageBox.Show(StringResources.GetString("PermissionDenied"), 
                                   StringResources.GetString("RenameError"), 
                                   MessageBoxButtons.OK, 
                                   MessageBoxIcon.Error);
                    return;
                }
                
                // 4. For virtual mode, directly rename through file operations
                // This avoids using SelectedItems collection which is not accessible in virtual mode
                if (browser.VirtualMode)
                {
                    Logger.Debug("ShellBrowser.RenameSelectedItem: In VirtualMode, directly renaming file/directory");
                    string oldName = Path.GetFileName(fileInfo.FullName);
                    string newName = null;
                    
                    // Use FormNewFolder dialog for renaming
                    using (FormNewFolder renameDialog = new FormNewFolder())
                    {
                        renameDialog.Text = StringResources.GetString("RenameTitle");
                        renameDialog.NewName = oldName;
                        renameDialog.Init();
                        
                        if (FormHelper.ShowDialogCentered(renameDialog, this.FindForm()) == DialogResult.OK)
                        {
                            newName = renameDialog.NewName;
                        }
                    }
                    
                    // If user entered a new name
                    if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
                    {
                        try
                        {
                            string targetPath = Path.Combine(Path.GetDirectoryName(fileInfo.FullName), newName);
                            
                            // Rename file or directory
                            if (fileInfo is FileInfo)
                            {
                                File.Move(fileInfo.FullName, targetPath);
                            }
                            else if (fileInfo is DirectoryInfo)
                            {
                                Directory.Move(fileInfo.FullName, targetPath);
                            }
                            
                            // Refresh and maintain selection
                            RefreshListView();
                            // Try to reselect item with the new name
                            for (int i = 0; i < m_ShellItemInfo.Count; i++)
                            {
                                if (Path.GetFileName(m_ShellItemInfo[i].FullName).Equals(newName, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (i < browser.Items.Count)
                                    {
                                        browser.Items[i].Selected = true;
                                        browser.Items[i].Focused = true;
                                        browser.EnsureVisible(i);
                                    }
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "ShellBrowser.RenameSelectedItem: Error during rename operation");
                            MessageBox.Show(StringResources.GetString("RenameFailed") + ex.Message,
                                          StringResources.GetString("Error"),
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Error);
                        }
                    }
                    return;
                }
                
                // 5. For standard mode, use LabelEdit functionality
                Logger.Debug($"ShellBrowser.RenameSelectedItem: Setting LabelEdit=true for item at index {selectedIdx}");
                browser.LabelEdit = true;
                
                // 6. Ensure item is visible
                browser.EnsureVisible(selectedIdx);
                
                // 7. Set focus and start edit
                browser.FocusedItem = browser.Items[selectedIdx];
                browser.Items[selectedIdx].BeginEdit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ShellBrowser.RenameSelectedItem: Exception occurred during rename operation");
                MessageBox.Show(StringResources.GetString("RenameFailed") + ex.Message,
                               StringResources.GetString("Error"),
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shows a dialog centered on the main form
        /// </summary>
        private DialogResult ShowDialogCentered(Form dialog)
        {
            Form mainForm = this.FindForm();
            if (mainForm != null)
            {
                return FormHelper.ShowDialogCentered(dialog, mainForm);
            }
            else
            {
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.Owner = mainForm;
                return dialog.ShowDialog();
            }
        }

        /// <summary>
        /// Apply the sort mode to the file browser
        /// </summary>
        /// <param name="sortMode">Sort mode (0: Name, 1: Size, 2: Date, 3: Extension)</param>
        public void ApplySortMode(int sortMode)
        {
            // Set the sort column and order based on the sort mode
            switch (sortMode)
            {
                case 0: // Name
                    SortColumn = 0;
                    Order = SortOrder.Ascending;
                    break;
                case 1: // Size
                    SortColumn = 1;
                    Order = SortOrder.Descending;
                    break;
                case 2: // Date
                    SortColumn = 2;
                    Order = SortOrder.Descending;
                    break;
                case 3: // Extension
                    SortColumn = 3;
                    Order = SortOrder.Ascending;
                    break;
                default:
                    SortColumn = 0;
                    Order = SortOrder.Ascending;
                    break;
            }
            
            // Apply the sort if browser is already initialized
            if (browser != null && browser.Items.Count > 0)
            {
                // Refresh list view with the new sort settings
                RefreshListView();
                Logger.Debug($"Sort mode applied: {sortMode}");
            }
        }

        /// <summary>
        /// Check if the file has an archive extension
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
        /// Display archive file contents in current view
        /// </summary>
        private bool ProcessArchiveFile(string archivePath)
        {
            try
            {
                // Show temporary status
                SetStatusMessage(StringResources.GetString("LoadingArchiveContents"));
                
                // ZIP 파일만 지원 (기본 .NET 라이브러리로 처리 가능)
                if (Path.GetExtension(archivePath).ToLowerInvariant() != ".zip")
                {
                    CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                        StringResources.GetString("OnlyZipSupported"), 
                        StringResources.GetString("ArchiveViewer"), 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                
                // 임시 상태 표시
                SetStatusMessage("압축 파일 내용을 불러오는 중...");
                
                // 압축 파일 처리
                using (var archive = System.IO.Compression.ZipFile.OpenRead(archivePath))
                {
                    // 화면 업데이트 일시 중지
                    browser.BeginUpdate();
                    
                    try
                    {
                        // 캐시 및 데이터 초기화
                        m_ListItemCache = null;
                        m_FirstItem = 0;
                        m_ShellItemInfo.Clear();
                        
                        // 기존 항목 모두 제거
                        browser.VirtualListSize = 0;
                        
                        // 임시 저장 경로 지정 (압축 파일 내용을 메모리에만 유지하고 실제로 추출하지 않음)
                        string tempVirtualPath = "[" + Path.GetFileName(archivePath) + "]";
                        CurrentPath = tempVirtualPath;
                        txtPath.Text = tempVirtualPath;
                        
                        // 압축 파일 내용을 기반으로 가상 항목 생성
                        List<FileSystemInfo> archiveItems = new List<FileSystemInfo>();
                        
                        // 임시 디렉토리 및 파일 생성을 위한 기본 경로
                        string tempBasePath = Path.GetTempPath();
                        string tempWorkDir = Path.Combine(tempBasePath, "ArchiveView_" + Guid.NewGuid().ToString().Substring(0, 8));
                        
                        try
                        {
                            // 압축 파일 내부 탐색 상태 설정
                            m_IsInsideArchive = true;
                            m_CurrentArchivePath = archivePath;
                            m_TempWorkDir = tempWorkDir;
                            
                            // 임시 작업 디렉토리 생성
                            if (!Directory.Exists(tempWorkDir))
                                Directory.CreateDirectory(tempWorkDir);
                            
                            // 디렉토리 목록과 파일 목록 생성 (중복 제거)
                            Dictionary<string, bool> directories = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                            
                            // 우선 디렉토리 구조를 생성
                            foreach (var entry in archive.Entries)
                            {
                                try
                                {
                                    // 빈 디렉토리나 경로 구분자로 끝나는 경우 처리
                                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                                    {
                                        string dirPath = entry.FullName.TrimEnd('/', '\\');
                                        if (!string.IsNullOrEmpty(dirPath) && !directories.ContainsKey(dirPath))
                                        {
                                            // 디렉토리 생성
                                            string fullDirPath = Path.Combine(tempWorkDir, dirPath.Replace('/', Path.DirectorySeparatorChar));
                                            DirectoryInfo dirInfo = Directory.CreateDirectory(fullDirPath);
                                            
                                            // 폴더 항목으로 추가
                                            archiveItems.Add(dirInfo);
                                            directories[dirPath] = true;
                                        }
                                    }
                                    else
                                    {
                                        // 파일이 있는 디렉토리 처리
                                        string dirPath = Path.GetDirectoryName(entry.FullName);
                                        if (!string.IsNullOrEmpty(dirPath) && !directories.ContainsKey(dirPath))
                                        {
                                            // 디렉토리 생성
                                            string fullDirPath = Path.Combine(tempWorkDir, dirPath.Replace('/', Path.DirectorySeparatorChar));
                                            DirectoryInfo dirInfo = Directory.CreateDirectory(fullDirPath);
                                            
                                            // 폴더 항목으로 추가
                                            archiveItems.Add(dirInfo);
                                            directories[dirPath] = true;
                                        }
                                        
                                        // 파일 경로 설정
                                        string fullFilePath = Path.Combine(tempWorkDir, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
                                        
                                        // 빈 파일 생성하여 아이콘 표시 용도로만 사용
                                        if (!File.Exists(fullFilePath))
                                        {
                                            // 디렉토리가 없으면 생성
                                            string fileDir = Path.GetDirectoryName(fullFilePath);
                                            if (!Directory.Exists(fileDir))
                                                Directory.CreateDirectory(fileDir);
                                                
                                            // 0바이트 파일 생성
                                            using (File.Create(fullFilePath)) { }
                                        }
                                        
                                        // 파일 항목 추가
                                        FileInfo fileInfo = new FileInfo(fullFilePath);
                                        archiveItems.Add(fileInfo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex, $"Error processing archive entry: {entry.FullName}");
                                }
                            }
                            
                            // 가상 항목 추가
                            if (archiveItems.Count > 0)
                                m_ShellItemInfo.AddRange(archiveItems);
                            
                            // 리스트 크기 설정
                            browser.VirtualListSize = m_ShellItemInfo.Count;
                            
                            // 첫 번째 항목 선택
                            if (browser.VirtualListSize > 0)
                            {
                                browser.SelectedIndices.Clear();
                                browser.SelectedIndices.Add(0);
                            }
                            
                            // 화면 갱신
                            browser.Invalidate(true);
                            browser.Update();
                            
                            // 첫 항목 스크롤
                            if (browser.VirtualListSize > 0)
                            {
                                browser.EnsureVisible(0);
                                browser.FocusedItem = browser.Items[0];
                            }
                            
                            // 상태 메시지 업데이트
                            SetStatusMessage($"압축 파일: {Path.GetFileName(archivePath)} - {archive.Entries.Count}개 항목");
                            
                            // 압축 파일 히스토리 추가 (뒤로가기 지원)
                            m_History.Add(archivePath);
                            
                            // Update status message
                            SetStatusMessage(StringResources.GetString("ArchiveItemCount", Path.GetFileName(archivePath), archive.Entries.Count));
                            
                            // 압축 파일 히스토리 추가 (뒤로가기 지원)
                            m_History.Add(archivePath);
                            
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"Error creating temporary structures: {ex.Message}");
                            
                            // 압축 파일 내부 탐색 상태 해제
                            m_IsInsideArchive = false;
                            m_CurrentArchivePath = string.Empty;
                            
                            // 임시 디렉토리 정리
                            try
                            {
                                if (Directory.Exists(tempWorkDir))
                                    Directory.Delete(tempWorkDir, true);
                                
                                m_TempWorkDir = string.Empty;
                            }
                            catch { /* 무시 */ }
                            
                            throw;
                        }
                    }
                    finally
                    {
                        // 화면 업데이트 재개
                        browser.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error processing archive file: {archivePath}");
                CustomDialogHelper.ShowMessageBox(this.FindForm(), 
                    $"압축 파일을 처리하는 중 오류가 발생했습니다: {ex.Message}", 
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Returns full paths of selected items.
        /// </summary>
        /// <returns>Array of full paths of selected files/folders</returns>
        public string[] GetSelectedItems()
        {
            if (browser.SelectedIndices.Count == 0)
                return new string[0];

            List<string> selectedPaths = new List<string>();

            foreach (int index in browser.SelectedIndices)
            {
                if (index >= 0 && index < m_ShellItemInfo.Count)
                {
                    FileSystemInfo fileInfo = m_ShellItemInfo[index];
                    selectedPaths.Add(fileInfo.FullName);
                }
            }

            return selectedPaths.ToArray();
        }

        /// <summary>
        /// Refreshes the file list.
        /// </summary>
        public void RefreshContents()
        {
            RefreshListView();
        }

        /// <summary>
        /// Select newly created folder
        /// </summary>
        private void SelectCreatedFolder(string folderPath)
        {
            try
            {
                string folderName = Path.GetFileName(folderPath);
                
                // 항목 찾기
                for (int i = 0; i < m_ShellItemInfo.Count; i++)
                {
                    FileSystemInfo info = m_ShellItemInfo[i];
                    if (info is DirectoryInfo && info.Name == folderName)
                    {
                        // 선택 목록 초기화
                        browser.SelectedIndices.Clear();
                        
                        // 새 항목 선택
                        browser.SelectedIndices.Add(i);
                        browser.EnsureVisible(i);
                        browser.Focus();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error selecting created folder: {folderPath}");
            }
        }
        
        /// <summary>
        /// Select newly created file
        /// </summary>
        private void SelectCreatedFile(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                
                // 항목 찾기
                for (int i = 0; i < m_ShellItemInfo.Count; i++)
                {
                    FileSystemInfo info = m_ShellItemInfo[i];
                    if (info is FileInfo && info.Name == fileName)
                    {
                        // 선택 목록 초기화
                        browser.SelectedIndices.Clear();
                        
                        // 새 항목 선택
                        browser.SelectedIndices.Add(i);
                        browser.EnsureVisible(i);
                        browser.Focus();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error selecting created file: {filePath}");
            }
        }
    }
}
