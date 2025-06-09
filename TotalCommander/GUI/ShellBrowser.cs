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
                    // 삭제 확인 메시지 생성
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
                        
                        // 진행 상황 표시줄 추가
                        ProgressBar progressBar = new ProgressBar();
                        progressBar.Dock = DockStyle.None;
                        progressBar.Location = new Point(20, 20);
                        progressBar.Size = new Size(360, 23);
                        progressBar.Style = ProgressBarStyle.Marquee;
                        progressForm.Controls.Add(progressBar);
                        
                        // 상태 라벨 추가
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
            CopySelectedItems();
            CanCut = true;
        }

        #region Delegates
        private delegate void PasteFileDel(string source, string dest, UIOption uiOption, UICancelOption cancelOption);
        private delegate void PasteDirDel(string source, string dest, UIOption uiOption, UICancelOption cancelOption);
        #endregion Delegates
        /// <summary>
        /// 선택된 항목을 클립보드에 붙여넣기
        /// </summary>
        public void PasteFromClipboard()
        {
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

        public void CreateNewFile()
        {
            string name = GetDefaultDirectoryName(CurrentPath, "New Text Document");
            string filter = @"All file (*.*) | *.*";
            var dialog = new SaveFileDialog()
            {
                FileName = name,
                DefaultExt = ".txt",
                InitialDirectory = CurrentPath,
                ValidateNames = true,
                Filter = filter
            };
            
            // Show dialog using FormHelper
            Form mainForm = this.FindForm();
            if (FormHelper.ShowCommonDialog(dialog, mainForm) == DialogResult.OK)
            {
                try
                {
                    var s = File.Create(dialog.FileName);
                    s.Close();
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show(StringResources.GetString("InvalidFileName"));
                }
                catch (PathTooLongException)
                {
                    MessageBox.Show(StringResources.GetString("FileNameTooLong"));
                }
                catch (IOException)
                {
                    MessageBox.Show(StringResources.GetString("ReadOnlyParentDirectory"));
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(StringResources.GetString("CannotCreateFileInThisFolder"));
                }
                finally { RefreshListView(); }
            }
        }

        /// <summary>
        /// Creates new folder in current path
        /// </summary>
        public void CreateNewFolder()
        {
            string name = GetDefaultDirectoryName(CurrentPath);

            FormNewFolder frmNewfolder = new FormNewFolder() { NewName = name };
            frmNewfolder.Init();
            ShowDialogCentered(frmNewfolder);

            if (frmNewfolder.DialogResult == DialogResult.OK)
            {
                name = frmNewfolder.NewName;
                string fullPath = Path.Combine(CurrentPath, name);
                bool exists = Directory.Exists(fullPath);
                try
                {
                    if (!exists)
                    {
                        Directory.CreateDirectory(fullPath);
                        RefreshListView();
                    }
                    else
                    {
                        MessageBox.Show(StringResources.GetString("FolderAlreadyExists", name));
                    }
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show(StringResources.GetString("InvalidDirectoryName"));
                }
                catch (PathTooLongException)
                {
                    MessageBox.Show(StringResources.GetString("DirectoryNameTooLong"));
                }
                catch (IOException)
                {
                    MessageBox.Show(StringResources.GetString("ReadOnlyParentDirectory"));
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(StringResources.GetString("CannotCreateDirectoryInThisFolder"));
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
            if (ProcessFolder(path))
            {
                m_History.Add(path);
                txtPath.Text = CurrentPath; // Update address bar
            }
            else if (File.Exists(path))
            {
                Process.Start(path);
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

        public void GoBackward()
        {
            string path = m_History.Backward();
            if (!string.IsNullOrEmpty(path))
            {
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

        public void GoForward()
        {
            string path = m_History.Forward();
            if (!string.IsNullOrEmpty(path))
            {
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

        public void GoParent()
        {
            string parentPath = Directory.GetParent(CurrentPath)?.FullName;
            if (!string.IsNullOrEmpty(parentPath))
            {
                ProcessFileOrFolderPath(parentPath);
                m_History.Add(parentPath);
                
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
                    
                    // Use simple InputBox style dialog
                    using (Form inputDialog = new Form())
                    {
                        inputDialog.Text = StringResources.GetString("RenameTitle");
                        inputDialog.ClientSize = new Size(300, 80);
                        inputDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                        inputDialog.StartPosition = FormStartPosition.CenterParent;
                        inputDialog.MinimizeBox = false;
                        inputDialog.MaximizeBox = false;
                        
                        Label label = new Label();
                        label.Text = StringResources.GetString("FolderName");
                        label.Location = new Point(10, 10);
                        label.AutoSize = true;
                        inputDialog.Controls.Add(label);
                        
                        TextBox textBox = new TextBox();
                        textBox.Location = new Point(10, 30);
                        textBox.Size = new Size(280, 23);
                        textBox.Text = oldName;
                        textBox.SelectAll();
                        inputDialog.Controls.Add(textBox);
                        
                        Button buttonOk = new Button();
                        buttonOk.Text = StringResources.GetString("OK");
                        buttonOk.DialogResult = DialogResult.OK;
                        buttonOk.Location = new Point(110, 55);
                        inputDialog.Controls.Add(buttonOk);
                        
                        Button buttonCancel = new Button();
                        buttonCancel.Text = StringResources.GetString("Cancel");
                        buttonCancel.DialogResult = DialogResult.Cancel;
                        buttonCancel.Location = new Point(210, 55);
                        inputDialog.Controls.Add(buttonCancel);
                        
                        inputDialog.AcceptButton = buttonOk;
                        inputDialog.CancelButton = buttonCancel;
                        
                        if (FormHelper.ShowDialogCentered(inputDialog, this.FindForm()) == DialogResult.OK)
                        {
                            newName = textBox.Text;
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
    }
}
