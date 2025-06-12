using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TotalCommander;

namespace TotalCommander.GUI.Settings
{
    public partial class UserCommandPanel : SettingsPanelBase
    {
        private UserCommandSettings _commandSettings;
        private ListViewItem _selectedItem;
        private bool _isEditMode = false;

        public UserCommandPanel()
        {
            InitializeComponent();
            SetPanelName("사용자 명령");
        }

        private void UserCommandPanel_Load(object sender, EventArgs e)
        {
            LoadCommandSettings();
            UpdateEditPanel(null);
        }

        /// <summary>
        /// 설정 로드
        /// </summary>
        public override void LoadSettings()
        {
            try
            {
                LoadCommandSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("사용자 명령 설정 로드 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public override void SaveSettings()
        {
            try
            {
                if (_commandSettings != null)
                {
                    _commandSettings.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("사용자 명령 설정 저장 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 사용자 명령 설정 로드 및 목록 표시
        /// </summary>
        private void LoadCommandSettings()
        {
            _commandSettings = UserCommandSettings.Load();
            if (_commandSettings == null)
            {
                _commandSettings = new UserCommandSettings();
            }

            // 리스트뷰 초기화
            listViewCommands.Items.Clear();
            
            // 명령 목록 추가
            foreach (var command in _commandSettings.GetCommands())
            {
                ListViewItem item = new ListViewItem(command.Name);
                item.SubItems.Add(command.Path);
                item.SubItems.Add(command.Parameters);
                item.Tag = command;
                
                listViewCommands.Items.Add(item);
            }
        }

        /// <summary>
        /// 편집 패널 상태 업데이트
        /// </summary>
        private void UpdateEditPanel(ListViewItem item)
        {
            if (item == null)
            {
                txtName.Text = "";
                txtPath.Text = "";
                txtParams.Text = "";
                btnSave.Enabled = false;
                btnDelete.Enabled = false;
                _selectedItem = null;
                _isEditMode = false;
                return;
            }
            
            _selectedItem = item;
            UserCommand command = (UserCommand)item.Tag;
            
            txtName.Text = command.Name;
            txtPath.Text = command.Path;
            txtParams.Text = command.Parameters;
            
            btnSave.Enabled = true;
            btnDelete.Enabled = true;
            _isEditMode = true;
        }

        /// <summary>
        /// 리스트뷰 선택 변경 이벤트
        /// </summary>
        private void listViewCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCommands.SelectedItems.Count > 0)
            {
                UpdateEditPanel(listViewCommands.SelectedItems[0]);
            }
            else
            {
                UpdateEditPanel(null);
            }
        }

        /// <summary>
        /// 저장 버튼 클릭 이벤트
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            // 입력 검증
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("명령 이름을 입력하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtPath.Text))
            {
                MessageBox.Show("실행 파일 경로를 입력하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPath.Focus();
                return;
            }
            
            // 새 명령 또는 기존 명령 수정
            if (_isEditMode && _selectedItem != null)
            {
                // 기존 명령 수정
                UserCommand command = (UserCommand)_selectedItem.Tag;
                command.Name = txtName.Text;
                command.Path = txtPath.Text;
                command.Parameters = txtParams.Text;
                
                // 목록 업데이트
                _selectedItem.SubItems[0].Text = command.Name;
                _selectedItem.SubItems[1].Text = command.Path;
                _selectedItem.SubItems[2].Text = command.Parameters;
                
                MessageBox.Show("명령이 수정되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // 새 명령 추가
                UserCommand command = new UserCommand
                {
                    Name = txtName.Text,
                    Path = txtPath.Text,
                    Parameters = txtParams.Text
                };
                
                _commandSettings.AddCommand(command);
                
                // 목록에 추가
                ListViewItem item = new ListViewItem(command.Name);
                item.SubItems.Add(command.Path);
                item.SubItems.Add(command.Parameters);
                item.Tag = command;
                
                listViewCommands.Items.Add(item);
                listViewCommands.SelectedItems.Clear();
                item.Selected = true;
                
                MessageBox.Show("새 명령이 추가되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            // 설정 저장
            _commandSettings.Save();
        }

        /// <summary>
        /// 삭제 버튼 클릭 이벤트
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null) return;
            
            if (MessageBox.Show("선택한 명령을 삭제하시겠습니까?", "확인", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                UserCommand command = (UserCommand)_selectedItem.Tag;
                
                // 명령 삭제
                _commandSettings.RemoveCommand(command);
                
                // 목록에서 제거
                listViewCommands.Items.Remove(_selectedItem);
                
                // 편집 패널 초기화
                UpdateEditPanel(null);
                
                // 설정 저장
                _commandSettings.Save();
                
                MessageBox.Show("명령이 삭제되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 찾아보기 버튼 클릭 이벤트
        /// </summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*";
                dlg.Title = "실행 파일 선택";
                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = dlg.FileName;
                }
            }
        }

        /// <summary>
        /// 새로 만들기 버튼 클릭 이벤트
        /// </summary>
        private void btnNew_Click(object sender, EventArgs e)
        {
            txtName.Text = "";
            txtPath.Text = "";
            txtParams.Text = "";
            
            listViewCommands.SelectedItems.Clear();
            _selectedItem = null;
            _isEditMode = false;
            
            btnSave.Enabled = true;
            btnDelete.Enabled = false;
            
            txtName.Focus();
        }
    }
} 