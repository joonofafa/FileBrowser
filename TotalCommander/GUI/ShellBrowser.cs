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
        /// 파일 탐색기 컨트롤에 대한 접근자
        /// </summary>
        public ListView FileExplorer => browser;
        #endregion

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
            
            // 초기화 후 첫 번째 항목 선택
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
            
            // 경로 표시 영역이 숨겨지므로 내비게이션 창이 전체 높이를 사용하도록 설정
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
                
                // 첫 번째 항목 선택
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
            txtPath.Visible = false; // 텍스트 경로 표시 영역 숨기기
            lblTopStorageStatus.Visible = false; // 상단 저장소 상태 레이블 숨기기
            disksBrowser.Visible = false; // 디스크 브라우저 숨기기
            
            // 모든 상단 컨트롤이 숨겨진 상태에서 splMainView가 전체 영역을 차지하도록 설정
            splMainView.Location = new System.Drawing.Point(0, 0);
            splMainView.Height = this.Height - lblBotStatus.Height;
            
            txtPath.LostFocus += TxtPath_LostFocus;
            txtPath.KeyDown += TxtPath_KeyDown;
        }

        void TxtPath_LostFocus(object sender, EventArgs e)
        {
            bool isSame = txtPath.Text.Equals(CurrentPath);
            if (!isSame)
                txtPath.Text = CurrentPath;
        }

        void TxtPath_KeyDown(object sender, KeyEventArgs e)
        {
            Keys ourKey = e.KeyData;
            switch (e.KeyData)
            {
                case Keys.Enter:
                    txtPath.Text = txtPath.Text.Trim();
                    if (txtPath.Text.Contains(Path.DirectorySeparatorChar) && ProcessFolder(txtPath.Text))
                        m_History.Add(txtPath.Text);
                    else
                        txtPath.Text = CurrentPath;
                    ComboBoxItem cbi = (ComboBoxItem)disksBrowser.SelectedItem;
                    string path = cbi.Text;
                    RefreshDisksBrowser(CurrentPath, path);
                    break;
                case Keys.Escape:
                    txtPath.Text = CurrentPath;
                    txtPath.SelectionStart = CurrentPath.Length;
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
            
            // 이벤트 핸들러 등록
            browser.SelectedIndexChanged -= Browser_SelectedIndexChanged; // 기존에 등록된 핸들러가 있으면 제거
            browser.SelectedIndexChanged += Browser_SelectedIndexChanged;
            
            // 가상 모드에서 선택 변경을 추가로 감지하기 위한 이벤트
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
                case 0: goto case 3; break;
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
                    warningMsg = "The path is not valid for one of the following reasons:" +
                    "it is a zero-length string; it contains only white space; it contains" +
                    @"invalid characters; or it is a device path (starts with \\.\).";
                }
                catch (FileNotFoundException)
                {
                    warningMsg = "This file or directory not exists.";
                }
                catch (PathTooLongException)
                {
                    warningMsg = "The file name exceeds system-defined maximum length.";
                }
                catch (IOException) { warningMsg = "The new file name already exists."; }
                catch (NotSupportedException) { warningMsg = "Invalid name"; }
                catch (UnauthorizedAccessException) { warningMsg = "You don't have enough permission on this file or folder."; }
                finally
                {
                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        e.CancelEdit = true;
                        MessageBox.Show(warningMsg, "Total Commander", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show(ex.StackTrace);
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
                case Keys.Space:
                    if (browser.SelectedIndices.Count == 0)
                    {
                        browser.SelectedIndices.Add(0);
                        browser.EnsureVisible(0);
                    }
                    break;
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
            // 디버깅 메시지 (필요시 주석 해제)
            // MessageBox.Show("선택 변경됨!");

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
                statusText = $"Selected {selectedFiles} file(s), {selectedDirs} dir(s) | Total size: {FormatBytes(totalSize)}";
            }
            else if (selectedFiles > 0)
            {
                statusText = $"Selected {selectedFiles} file(s) | Total size: {FormatBytes(totalSize)}";
            }
            else if (selectedDirs > 0)
            {
                statusText = $"Selected {selectedDirs} dir(s)";
            }
            else
            {
                UpdateFolderSummaryStatus();
                return;
            }

            // 먼저 직접 레이블 업데이트 
            lblBotStatus.Text = statusText;
            
            // 그리고 이벤트도 발생시킴
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
                    string statusText = $"{files} file(s), {folders} dir(s)";
                    StatusChanged?.Invoke(this, new StatusChangedEventArgs(statusText));
                }
                catch (Exception)
                {
                    string errorText = "Error reading folder contents";
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
                
                // 첫 번째 항목 선택
                SelectFirstItem();
                
                // 폴더 목록창 동기화
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

        delegate void DeleteFileDel(string file, UIOption showUI, RecycleOption recycle, UICancelOption onUserCancel);
        public async void DeleteSelectedItems(RecycleOption recycle)
        {
            if (browser.SelectedIndices.Count == 0)
                return;

            string message = string.Empty;
            string caption = "Confirm Delete";

            int selectedCount = browser.SelectedIndices.Count;

            if (recycle == RecycleOption.SendToRecycleBin)
            {
                message = string.Format("Are you sure you want to move {0} item(s) to the Recycle Bin?", selectedCount);
            }
            else // DeletePermanently
            {
                message = string.Format("Are you sure you want to permanently delete {0} item(s)?", selectedCount);
            }

            DialogResult result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return; // 사용자가 '아니오'를 선택하면 삭제하지 않음
            }

            List<FileSystemInfo> itemsToDelete = new List<FileSystemInfo>();
            foreach (int selectedIndex in browser.SelectedIndices)
            {
                if (selectedIndex >= 0 && selectedIndex < m_ShellItemInfo.Count) // 인덱스 유효성 검사
                {
                    itemsToDelete.Add(m_ShellItemInfo[selectedIndex]);
                }
            }

            if (itemsToDelete.Count == 0) // 실제로 삭제할 항목이 없는 경우
            {
                return;
            }

            List<Task> taskList = new List<Task>();
            foreach (FileSystemInfo itemInfo in itemsToDelete) // FileSystemInfo 객체를 사용
            {
                string fileName = itemInfo.FullName; // FileSystemInfo에서 FullName 사용
                DeleteFileDel deleteFunc = null;
                if (File.Exists(fileName))
                    deleteFunc = FileSystem.DeleteFile;
                else if (Directory.Exists(fileName))
                    deleteFunc = FileSystem.DeleteDirectory;

                if (deleteFunc != null)
                {
                    Task task = Task.Run(() => deleteFunc(fileName, UIOption.AllDialogs, recycle, UICancelOption.ThrowException));
                    taskList.Add(task);
                }
            }

            foreach (Task currentTask in taskList)
            {
                try
                {
                    await currentTask;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace);
                }
                finally
                {
                    RefreshListView();
                }
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
                    var dialog = new FormProgressCopy(files, destPath, CanCut);
                    dialog.ShowDialog();
                    RefreshListView();
                }
            }
        }

        private void RefreshListView()
        {
            ProcessFolder(CurrentPath);
            
            // 목록을 새로 고친 후 첫 번째 항목 선택
            SelectFirstItem();
            
            // 폴더 목록창 동기화
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
                        var result = frmPacking.ShowDialog(this.FindForm());
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
            frmFind.ShowDialog(this.FindForm());
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
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var s = File.Create(dialog.FileName);
                    s.Close();
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("Invalid file name");
                }
                catch (PathTooLongException)
                {
                    MessageBox.Show("The file name is too long");
                }
                catch (IOException)
                {
                    MessageBox.Show("The parent directory of the file to be created is read-only");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Cannot create file in this folder");
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
            frmNewfolder.ShowDialog(this.FindForm());

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
                        MessageBox.Show("Directory already exists");
                    }
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("Invalid directory name");
                }
                catch (PathTooLongException)
                {
                    MessageBox.Show("The directory name is too long");
                }
                catch (IOException)
                {
                    MessageBox.Show("The parent directory of the directory to be created is read-only");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Cannot create directory in this folder");
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
                MessageBox.Show("Access denied on file" + Environment.NewLine + path.FullName,
                    "Total Commander", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    txtPath.Text = CurrentPath;
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
        private bool Navigate(string path)
        {
            var currentDir = new DirectoryInfo(path);
            var subDirs = GetSubDirectories(currentDir);
            if (null == subDirs)
            {
                MessageBox.Show("Access denied on folder" + Environment.NewLine + currentDir.FullName,
                    "Total Commander", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            CurrentPath = path;
            m_ListItemCache = null;
            m_ShellItemInfo.Clear();
            var subFiles = GetSubFiles(currentDir);
            if (subDirs.Length > 0)
                m_ShellItemInfo.AddRange(subDirs);
            if (subFiles.Length > 0)
                m_ShellItemInfo.AddRange(subFiles);
            browser.VirtualListSize = 0;
            browser.Invalidate();
            browser.VirtualListSize = m_ShellItemInfo.Count;
            
            // 첫 번째 항목 선택
            SelectFirstItem();
            
            // 파일 표시창 경로가 변경되면 폴더 목록창에도 반영
            SyncNavigationPaneWithCurrentPath();
            
            return true;
        }
        
        /// <summary>
        /// 현재 경로를 기준으로 폴더 목록창(NavigationPane)의 선택된 노드를 동기화합니다.
        /// </summary>
        private void SyncNavigationPaneWithCurrentPath()
        {
            if (!m_HideNavigationPane && !string.IsNullOrEmpty(CurrentPath))
            {
                // NavigationPane에 현재 경로 반영
                navigationPane.SelectNodeByPath(CurrentPath);
            }
        }
        
        /// <summary>
        /// 첫 번째 항목을 선택합니다.
        /// </summary>
        private void SelectFirstItem()
        {
            if (browser.VirtualListSize > 0)
            {
                browser.SelectedIndices.Clear();
                browser.SelectedIndices.Add(0);  // 첫 번째 항목(인덱스 0) 선택
                browser.EnsureVisible(0);        // 보이게 스크롤
                browser.FocusedItem = browser.Items[0]; // 포커스 설정
            }
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
                    
                    // 첫 번째 항목 선택
                    SelectFirstItem();
                    
                    // 폴더 목록창 동기화
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
                    
                    // 첫 번째 항목 선택
                    SelectFirstItem();
                    
                    // 폴더 목록창 동기화
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
                
                // 첫 번째 항목 선택
                SelectFirstItem();
                
                // 폴더 목록창 동기화
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

            // 파일 목록 ListView의 폰트 변경
            if (browser != null)
            {
                browser.Font = font;
            }

            // 폴더 트리 NavigationPane의 폰트 변경
            if (navigationPane != null)
            {
                navigationPane.Font = font;
            }
            // 경로 표시 TextBox 폰트도 변경 (선택 사항)
            if (txtPath != null)
            {
                txtPath.Font = font;
            }
        }

        /// <summary>
        /// 하단 상태창 폰트를 설정합니다.
        /// </summary>
        /// <param name="font">적용할 폰트</param>
        public void ApplyStatusBarFont(Font font)
        {
            if (font == null || lblBotStatus == null) return;
            
            // 상태 표시줄의 폰트를 변경합니다. 
            // 메인 폰트보다 더 작게 설정하여 공간을 절약합니다.
            float newSize = Math.Max(font.Size - 2, 7); // 최소 7pt, 메인 폰트보다 2pt 작게
            Font statusFont = new Font(font.FontFamily, newSize, font.Style);
            lblBotStatus.Font = statusFont;
            
            // 폰트 크기에 맞게 상태바 높이를 조정합니다.
            AdjustStatusBarHeight(statusFont);
        }
        
        /// <summary>
        /// 폰트 크기에 맞게 상태바 높이를 조정합니다.
        /// </summary>
        /// <param name="font">상태바에 적용된 폰트</param>
        private void AdjustStatusBarHeight(Font font)
        {
            if (lblBotStatus == null || font == null) return;
            
            // 기본 여백 (상하 패딩)
            const int padding = 3;
            
            // 폰트 크기에 기반한 새 높이 계산 - 더 작게 조정
            int newHeight = (int)Math.Ceiling(font.Height * 1.0) + padding;
            
            // 최소 높이 제한 - 작게 설정
            newHeight = Math.Max(newHeight, 16);
            
            // 현재 높이와 다른 경우에만 조정
            if (lblBotStatus.Height != newHeight)
            {
                // 상태바 크기 및 위치 조정
                lblBotStatus.Height = newHeight;
                lblBotStatus.Location = new System.Drawing.Point(0, this.Height - newHeight);
                
                // 컨트롤 레이아웃 업데이트
                this.PerformLayout();
                
                // 상태바와 브라우저 패널 사이의 간격을 조정
                if (splMainView != null)
                {
                    // splMainView 크기 조정 (상태바 위에 맞추기)
                    splMainView.Height = this.Height - newHeight - splMainView.Location.Y;
                }
                
                // 변경 내용을 화면에 반영
                lblBotStatus.Invalidate();
                this.Invalidate();
            }
        }

        public void FocusNavigationPane()
        {
            // Focus() 메서드로는 부족할 수 있으므로 여러 방법을 시도
            if (this.navigationPane != null)
            {
                // 1. 먼저 사용자 지정 방식으로 포커스 설정 시도
                this.navigationPane.Select();
                this.navigationPane.Focus();
                
                // 2. 포커스가 설정되었는지 확인 (디버깅용)
                // MessageBox.Show("네비게이션 패널 포커스: " + (this.navigationPane.Focused ? "성공" : "실패"));
            }
        }

        public void FocusFileBrowser()
        {
            // Focus() 메서드로는 부족할 수 있으므로 여러 방법을 시도
            if (this.browser != null)
            {
                // 1. 먼저 사용자 지정 방식으로 포커스 설정 시도
                this.browser.Select();
                this.browser.Focus();
                
                // 2. 포커스가 설정되었는지 확인 (디버깅용)
                // MessageBox.Show("파일 브라우저 포커스: " + (this.browser.Focused ? "성공" : "실패"));
            }
        }
        
        /// <summary>
        /// 현재 컬럼 너비 설정을 가져옵니다.
        /// </summary>
        /// <returns>컬럼 인덱스와 너비를 포함하는 딕셔너리</returns>
        public Dictionary<int, int> GetColumnWidths()
        {
            if (browser != null && browser.View == View.Details)
            {
                return browser.GetColumnWidths();
            }
            return new Dictionary<int, int>();
        }
        
        /// <summary>
        /// 저장된 컬럼 너비 설정을 적용합니다.
        /// </summary>
        /// <param name="columnWidths">적용할 컬럼 너비 설정</param>
        public void ApplyColumnWidths(Dictionary<int, int> columnWidths)
        {
            if (browser != null && browser.View == View.Details && columnWidths != null)
            {
                browser.ApplyColumnWidths(columnWidths);
            }
        }

        // 추가: 마우스 클릭 이벤트를 통한 선택 변경 감지
        private void Browser_MouseClick(object sender, MouseEventArgs e)
        {
            // 클릭 이벤트가 발생하면 선택이 변경되었을 가능성이 있으므로, SelectedIndexChanged 핸들러를 수동으로 호출
            Browser_SelectedIndexChanged(sender, EventArgs.Empty);
        }
        
        // 추가: 키보드 이벤트를 통한 선택 변경 감지
        private void Browser_KeyUp(object sender, KeyEventArgs e)
        {
            // 방향키, Space, Enter 등 선택에 영향을 줄 수 있는 키가 눌렸을 때
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || 
                e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Home || e.KeyCode == Keys.End ||
                e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
            {
                // 선택이 변경되었을 가능성이 있으므로, SelectedIndexChanged 핸들러를 수동으로 호출
                Browser_SelectedIndexChanged(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 컬럼 너비 변경 이벤트를 등록하는 메서드
        /// </summary>
        /// <param name="handler">컬럼 너비 변경 이벤트 핸들러</param>
        public void RegisterColumnWidthChangedEvent(EventHandler handler)
        {
            if (browser != null && handler != null)
            {
                browser.ColumnWidthChanged += handler;
            }
        }

        /// <summary>
        /// 컬럼 너비 변경 이벤트를 해제하는 메서드
        /// </summary>
        /// <param name="handler">컬럼 너비 변경 이벤트 핸들러</param>
        public void UnregisterColumnWidthChangedEvent(EventHandler handler)
        {
            if (browser != null && handler != null)
            {
                browser.ColumnWidthChanged -= handler;
            }
        }

        /// <summary>
        /// SplitterDistance 값을 가져옵니다.
        /// </summary>
        /// <returns>현재 SplitterDistance 값</returns>
        public int GetSplitterDistance()
        {
            if (splMainView != null)
            {
                return splMainView.SplitterDistance;
            }
            return 0;
        }
        
        /// <summary>
        /// SplitterDistance 값을 설정합니다.
        /// </summary>
        /// <param name="distance">설정할 SplitterDistance 값</param>
        public void SetSplitterDistance(int distance)
        {
            if (splMainView != null && distance > 0)
            {
                // 최소값과 최대값 사이로 제한
                int minDistance = splMainView.Panel1MinSize;
                int maxDistance = splMainView.Width - splMainView.Panel2MinSize - splMainView.SplitterWidth;
                
                // 유효한 범위로 조정
                distance = Math.Max(minDistance, Math.Min(distance, maxDistance));
                
                // SplitterDistance 설정
                splMainView.SplitterDistance = distance;
            }
        }
        
        /// <summary>
        /// SplitterDistance 변경 이벤트를 구독합니다.
        /// </summary>
        /// <param name="handler">이벤트 핸들러</param>
        public void RegisterSplitterDistanceChangedEvent(SplitterEventHandler handler)
        {
            if (splMainView != null && handler != null)
            {
                splMainView.SplitterMoved += handler;
            }
        }
        
        /// <summary>
        /// SplitterDistance 변경 이벤트 구독을 해제합니다.
        /// </summary>
        /// <param name="handler">이벤트 핸들러</param>
        public void UnregisterSplitterDistanceChangedEvent(SplitterEventHandler handler)
        {
            if (splMainView != null && handler != null)
            {
                splMainView.SplitterMoved -= handler;
            }
        }

        /// <summary>
        /// 선택된 파일이나 폴더의 이름을 변경합니다.
        /// </summary>
        public void RenameSelectedItem()
        {
            if (browser.SelectedIndices.Count != 1)
            {
                MessageBox.Show("이름을 변경하려면 하나의 항목만 선택해야 합니다.", "이름 변경", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int selectedIndex = browser.SelectedIndices[0];
            if (selectedIndex < 0 || selectedIndex >= m_ShellItemInfo.Count)
                return;

            FileSystemInfo selectedItem = m_ShellItemInfo[selectedIndex];
            string oldPath = selectedItem.FullName;
            string oldName = selectedItem.Name;
            string parentPath = Path.GetDirectoryName(oldPath);
            
            // 입력 대화상자를 사용하여 새 이름을 받음
            using (var dialog = new Form())
            {
                dialog.Text = "이름 변경";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.Width = 405; // 빨간색 선에 맞게 폼 너비 조정
                dialog.Height = 130;
                dialog.ShowInTaskbar = false;
                dialog.KeyPreview = true; // 키 이벤트를 폼에서 먼저 처리하도록 설정
                
                // ESC 키 처리를 위한 이벤트 핸들러 추가
                dialog.KeyDown += (s, e) => {
                    if (e.KeyCode == Keys.Escape)
                    {
                        dialog.DialogResult = DialogResult.Cancel;
                        dialog.Close();
                    }
                };

                // 텍스트박스 위치 조정 (라벨 제거로 인해)
                var textBox = new TextBox
                {
                    Text = oldName,
                    Left = 12,
                    Top = 20,
                    Width = 370,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                
                // 텍스트박스에 모든 텍스트 선택
                textBox.SelectAll();
                
                var okButton = new Button
                {
                    Text = "저장(&S)",  // Alt+S 단축키를 위한 &S 표기
                    DialogResult = DialogResult.OK,
                    Left = 227,
                    Top = 60,
                    Width = 75
                };
                
                var cancelButton = new Button
                {
                    Text = "닫기(&C)",  // Alt+C 단축키를 위한 &C 표기
                    DialogResult = DialogResult.Cancel,
                    Left = 308,
                    Top = 60,
                    Width = 75
                };
                
                dialog.Controls.Add(textBox);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                
                dialog.AcceptButton = okButton;    // Enter 키로 확인 버튼 클릭
                dialog.CancelButton = cancelButton; // ESC 키로 취소 버튼 클릭
                
                // 폼 보여주기
                DialogResult result = dialog.ShowDialog();
                
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text) && textBox.Text != oldName)
                {
                    try
                    {
                        string newName = textBox.Text;
                        string newPath = Path.Combine(parentPath, newName);
                        
                        // 동일한 이름이 이미 존재하는지 확인
                        if (selectedItem is DirectoryInfo && Directory.Exists(newPath))
                        {
                            MessageBox.Show($"이미 '{newName}' 폴더가 존재합니다.", "이름 변경 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else if (selectedItem is FileInfo && File.Exists(newPath))
                        {
                            MessageBox.Show($"이미 '{newName}' 파일이 존재합니다.", "이름 변경 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        // 파일이나 폴더 이름 변경
                        if (selectedItem is DirectoryInfo)
                        {
                            Directory.Move(oldPath, newPath);
                        }
                        else
                        {
                            File.Move(oldPath, newPath);
                        }
                        
                        // 목록 새로고침
                        RefreshListView();
                        
                        // 이름이 변경된 항목 선택
                        for (int i = 0; i < m_ShellItemInfo.Count; i++)
                        {
                            if (m_ShellItemInfo[i].FullName.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                            {
                                browser.SelectedIndices.Clear();
                                browser.SelectedIndices.Add(i);
                                browser.EnsureVisible(i);
                                browser.FocusedItem = browser.Items[i];
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이름을 변경하는 중 오류가 발생했습니다: {ex.Message}", "이름 변경 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
