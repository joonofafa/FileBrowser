using System;
using System.Windows.Forms;
using System.Collections.Generic;
using TotalCommander;

namespace TotalCommander.GUI
{
    internal class FileBrowser : ListView
    {
        #region Fields
        //private ListViewColumnSorter m_ColumnSorter = new ListViewColumnSorter();
        
        // Column width change event
        public new event EventHandler ColumnWidthChanged;
        #endregion Fields

        #region Overrided functions

        protected override bool IsInputKey(Keys keyData)
        {
            // Add key event logging
            Logger.Debug($"FileBrowser.IsInputKey called: keyData={keyData}");
            
            // Special handling for Space key
            if (keyData == Keys.Space)
            {
                Logger.Debug("FileBrowser.IsInputKey: Space key custom handling");
                return true;
            }
            
            // Default handling for other keys
            return base.IsInputKey(keyData);
        }
        
        // Custom processing for messages
        public override bool PreProcessMessage(ref Message msg)
        {
            const int WM_KEYDOWN = 0x0100;
            const int VK_SPACE = 0x20;
            
            // Check if Space key is pressed
            if (msg.Msg == WM_KEYDOWN && msg.WParam.ToInt32() == VK_SPACE)
            {
                Logger.Debug("FileBrowser.PreProcessMessage: Space key pressed");
                
                // Simulate key press for Space key
                var e = new KeyEventArgs(Keys.Space);
                this.OnKeyDown(e);
                
                // Return true if Space key was handled
                if (e.Handled)
                {
                    Logger.Debug("FileBrowser.PreProcessMessage: Space key handled");
                    return true;
                }
            }
            
            return base.PreProcessMessage(ref msg);
        }

        #region Cannot use ListViewItemSorter in virtual mode
        //protected override void OnColumnClick(ColumnClickEventArgs e)
        //{
        //    // Determine if clicked column is already the column that is being sorted.
        //    if (e.Column == SortColumn)
        //    {
        //        // Reverse the current sort direction for this column.
        //        if (Order == SortOrder.Ascending)
        //            Order = SortOrder.Descending;
        //        else
        //            Order = SortOrder.Ascending;
        //    }
        //    else
        //    {
        //        // Set the column number that is to be sorted; default to ascending.
        //        SortColumn = e.Column;
        //        Order = SortOrder.Ascending;
        //    }
        //    // set the sort arrow to a particular column
        //    this.SetSortIcon(e.Column, Order);
        //    // Perform the sort with these new sort options.
        //    //this.Sort();
        //    //this.ListViewItemSorter = m_ColumnSorter;
        //    base.OnColumnClick(e);
        //}
        #endregion

        // Custom method to select an item
        private void SelectItem(int itemIndex)
        {
            Logger.Debug($"FileBrowser.SelectItem: itemIndex={itemIndex}");
            
            if (itemIndex < 0 || itemIndex >= this.Items.Count)
            {
                Logger.Debug("FileBrowser.SelectItem: Invalid item index");
                return;
            }
            
            // BeginUpdate to prevent UI from updating
            this.BeginUpdate();
            
            try
            {
                // Select the item
                if (!this.SelectedIndices.Contains(itemIndex))
                {
                    Logger.Debug($"FileBrowser.SelectItem: Selecting item {itemIndex}");
                    this.SelectedIndices.Add(itemIndex);
                    this.Items[itemIndex].Selected = true;
                }
                
                // Focus on the selected item
                this.FocusedItem = this.Items[itemIndex];
                
                // Raise event for selected index change
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
            finally
            {
                // EndUpdate to allow UI to update
                this.EndUpdate();
                
                // Invalidate and update the UI
                this.Invalidate(true);
                this.Update();
                Application.DoEvents(); // UI update handling
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Add key event logging
            Logger.Debug($"FileBrowser.OnKeyDown: KeyCode={e.KeyCode}, KeyData={e.KeyData}, Handled={e.Handled}");
            
            switch (e.KeyData)
            {
                case Keys.Control | Keys.A:
                    Logger.Debug("FileBrowser: Ctrl+A key pressed - Select all");
                    for (int i = 0; i < this.VirtualListSize; i++)
                    {
                        this.SelectedIndices.Add(i);
                    }
                    this.Refresh();
                    e.Handled = true;
                    break;
                case Keys.Space:
                    Logger.Debug("FileBrowser: Space key pressed");
                    
                    // Select the focused item if it exists
                    if (this.FocusedItem != null)
                    {
                        int focusedIndex = this.FocusedItem.Index;
                        Logger.Debug($"FileBrowser: Focused item index={focusedIndex}");
                        
                        // 1. Select the focused item
                        SelectItem(focusedIndex);
                        
                        // 2. Select the next item (Down arrow key)
                        if (focusedIndex < this.Items.Count - 1)
                        {
                            int nextIndex = focusedIndex + 1;
                            this.FocusedItem = this.Items[nextIndex];
                            this.EnsureVisible(nextIndex);
                            Logger.Debug($"FileBrowser: Selected next item {nextIndex}");
                        }
                        
                        e.Handled = true;
                        Logger.Debug("FileBrowser: Space key handled, e.Handled=true");
                    }
                    else if (this.Items.Count > 0 && this.SelectedIndices.Count == 0)
                    {
                        // Select the first item if no items are selected
                        Logger.Debug("FileBrowser: Selecting first item");
                        SelectItem(0);
                        
                        // Select the second item (if exists)
                        if (this.Items.Count > 1)
                        {
                            this.FocusedItem = this.Items[1];
                            this.EnsureVisible(1);
                        }
                        
                        e.Handled = true;
                    }
                    break;
            }
            
            // Raise base class OnKeyDown method if not handled
            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }
        
        // Column width change handling
        protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            base.OnColumnWidthChanged(e);
            
            // Notify column width change
            ColumnWidthChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Overrided functions

        internal void Init()
        {
            this.HideSelection = false;
            this.MultiSelect = true;
            this.AllowDrop = true;
            this.ShowGroups = false;
            this.FullRowSelect = true;
            this.LabelEdit = true;
            this.DoubleBuffered = true;

            DefineColumn();
            this.SetSortIcon(0, SortOrder.Ascending);

            this.Resize += FileBrowser_Resize;
        }
        
        private void DefineColumn()
        {
            this.View = View.Details;
            this.HeaderStyle = ColumnHeaderStyle.Clickable;
            this.Columns.Add("Name");      // 0
            this.Columns.Add("Ext", 40);   // 1
            this.Columns.Add("Size", 60);  // 2
            this.Columns.Add("Date", 100); // 3
            this.Columns.Add("Attr", 35);  // 4
            int tmp = this.Width - 235;
            this.Columns[0].Width = (tmp > 60) ? tmp : 60;
        }
        
        /// <summary>
        /// Apply column widths to the ListView
        /// </summary>
        /// <param name="columnWidths">Dictionary of column indices and widths</param>
        public void ApplyColumnWidths(Dictionary<int, int> columnWidths)
        {
            if (columnWidths == null || this.View != View.Details || this.Columns.Count == 0)
                return;
                
            foreach (var columnInfo in columnWidths)
            {
                int columnIndex = columnInfo.Key;
                int width = columnInfo.Value;
                
                if (columnIndex >= 0 && columnIndex < this.Columns.Count && width > 0)
                {
                    this.Columns[columnIndex].Width = width;
                }
            }
        }
        
        /// <summary>
        /// Get column widths from the ListView
        /// </summary>
        /// <returns>Dictionary of column indices and widths</returns>
        public Dictionary<int, int> GetColumnWidths()
        {
            Dictionary<int, int> columnWidths = new Dictionary<int, int>();
            
            if (this.View == View.Details && this.Columns.Count > 0)
            {
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    columnWidths[i] = this.Columns[i].Width;
                }
            }
            
            return columnWidths;
        }

        public void ChangeViewMode(View view)
        {
            if (this.View == view) return;
            switch (view)
            {
                case View.List:
                    this.HeaderStyle = ColumnHeaderStyle.None;
                    this.Columns.Clear();
                    this.View = View.List;
                    break;
                case View.Details:
                    DefineColumn();
                    break;
            }
        }

        /// <summary>
        /// Sets to Name columns be the longest column
        /// </summary>
        void FileBrowser_Resize(object sender, EventArgs e)
        {
            if (this.View == View.Details)
            {
                int total = 0;
                for (int i = 1; i < 5; i++)
                {
                    total += this.Columns[i].Width;
                }
                int tmp = this.Width - total;
                this.Columns[0].Width = (tmp > 60) ? tmp : 60;
                
                // Notify column width change
                ColumnWidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
    }
}

