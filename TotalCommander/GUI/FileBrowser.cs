using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TotalCommander.GUI
{
    internal class FileBrowser : ListView
    {
        #region Fields
        //private ListViewColumnSorter m_ColumnSorter = new ListViewColumnSorter();
        
        // 컬럼 너비 저장 관련 이벤트
        public new event EventHandler ColumnWidthChanged;
        #endregion Fields

        #region Overrided functions

        protected override bool IsInputKey(Keys keyData)
        {
            // 키 이벤트 로깅 추가
            Logger.Debug($"FileBrowser.IsInputKey 호출: keyData={keyData}");
            
            // 명시적으로 스페이스 키 처리
            if (keyData == Keys.Space)
            {
                Logger.Debug("FileBrowser.IsInputKey: Space 키 직접 처리");
                return true;
            }
            
            // 나머지는 기본 처리
            return base.IsInputKey(keyData);
        }
        
        // 메시지 전처리를 위한 메서드 오버라이드
        public override bool PreProcessMessage(ref Message msg)
        {
            const int WM_KEYDOWN = 0x0100;
            const int VK_SPACE = 0x20;
            
            // 스페이스 키에 대한 WM_KEYDOWN 메시지 확인
            if (msg.Msg == WM_KEYDOWN && msg.WParam.ToInt32() == VK_SPACE)
            {
                Logger.Debug("FileBrowser.PreProcessMessage: Space 키 메시지 감지");
                
                // 키 다운 이벤트를 수동으로 발생
                var e = new KeyEventArgs(Keys.Space);
                this.OnKeyDown(e);
                
                // 이벤트가 처리되었으면 true 반환
                if (e.Handled)
                {
                    Logger.Debug("FileBrowser.PreProcessMessage: Space 키 이벤트 처리 완료");
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

        // 항목을 선택하는 메서드
        private void SelectItem(int itemIndex)
        {
            Logger.Debug($"FileBrowser.SelectItem: 인덱스={itemIndex}");
            
            if (itemIndex < 0 || itemIndex >= this.Items.Count)
            {
                Logger.Debug("FileBrowser.SelectItem: 유효하지 않은 인덱스");
                return;
            }
            
            // BeginUpdate로 UI 갱신 일시 중지
            this.BeginUpdate();
            
            try
            {
                // 항상 선택 상태로 설정
                if (!this.SelectedIndices.Contains(itemIndex))
                {
                    Logger.Debug($"FileBrowser.SelectItem: 항목 선택 {itemIndex}");
                    this.SelectedIndices.Add(itemIndex);
                    this.Items[itemIndex].Selected = true;
                }
                
                // 포커스 유지
                this.FocusedItem = this.Items[itemIndex];
                
                // 선택 변경 이벤트 수동 발생
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
            finally
            {
                // EndUpdate로 UI 갱신 재개
                this.EndUpdate();
                
                // 변경 내용 화면에 강제 반영
                this.Invalidate(true);
                this.Update();
                Application.DoEvents(); // UI 이벤트 처리 강제
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // 키 이벤트 로깅 추가
            Logger.Debug($"FileBrowser.OnKeyDown: KeyCode={e.KeyCode}, KeyData={e.KeyData}, Handled={e.Handled}");
            
            switch (e.KeyData)
            {
                case Keys.Control | Keys.A:
                    Logger.Debug("FileBrowser: Ctrl+A 처리 - 모두 선택");
                    for (int i = 0; i < this.VirtualListSize; i++)
                    {
                        this.SelectedIndices.Add(i);
                    }
                    this.Refresh();
                    e.Handled = true;
                    break;
                case Keys.Space:
                    Logger.Debug("FileBrowser: Space 키 처리 시작");
                    
                    // 현재 포커스된 항목이 있으면 처리
                    if (this.FocusedItem != null)
                    {
                        int focusedIndex = this.FocusedItem.Index;
                        Logger.Debug($"FileBrowser: 포커스된 항목 인덱스={focusedIndex}");
                        
                        // 1. 현재 항목 선택
                        SelectItem(focusedIndex);
                        
                        // 2. 다음 항목으로 이동 (Down 키 효과)
                        if (focusedIndex < this.Items.Count - 1)
                        {
                            int nextIndex = focusedIndex + 1;
                            this.FocusedItem = this.Items[nextIndex];
                            this.EnsureVisible(nextIndex);
                            Logger.Debug($"FileBrowser: 다음 항목으로 이동 {nextIndex}");
                        }
                        
                        e.Handled = true;
                        Logger.Debug("FileBrowser: Space 키 처리 완료, e.Handled=true");
                    }
                    else if (this.Items.Count > 0 && this.SelectedIndices.Count == 0)
                    {
                        // 선택된 항목이 없으면 첫 번째 항목 선택
                        Logger.Debug("FileBrowser: 첫 번째 항목 선택");
                        SelectItem(0);
                        
                        // 두 번째 항목으로 이동 (있는 경우)
                        if (this.Items.Count > 1)
                        {
                            this.FocusedItem = this.Items[1];
                            this.EnsureVisible(1);
                        }
                        
                        e.Handled = true;
                    }
                    break;
            }
            
            // 베이스 클래스 메서드 호출하기 전에 이벤트가 처리되었는지 확인
            if (!e.Handled)
            {
                base.OnKeyDown(e);
            }
        }
        
        // 컬럼 너비 변경 감지를 위한 메서드 오버라이드
        protected override void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            base.OnColumnWidthChanged(e);
            
            // 외부 이벤트 핸들러에게 변경 사항 통보
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
        /// 컬럼 너비 설정을 적용합니다.
        /// </summary>
        /// <param name="columnWidths">컬럼 인덱스를 키로, 너비를 값으로 하는 딕셔너리</param>
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
        /// 현재 컬럼 너비 설정을 가져옵니다.
        /// </summary>
        /// <returns>컬럼 인덱스를 키로, 너비를 값으로 하는 딕셔너리</returns>
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
                
                // 크기 변경 후 컬럼 너비 변경 이벤트 발생
                ColumnWidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
    }
}
