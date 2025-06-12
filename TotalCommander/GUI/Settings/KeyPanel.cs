using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TotalCommander.GUI.Settings
{
    public partial class KeyPanel : SettingsPanelBase
    {
        private KeySettings _keySettings;
        private ListViewItem _selectedItem;
        private bool _initializing = true;
        private bool _isPanelInitialized = false;

        public KeyPanel()
        {
            InitializeComponent();
            SetPanelName("기능 키");
        }

        private void InitializePanel()
        {
            if (_isPanelInitialized) return;
            InitActionComboBox();
            LoadKeySettings();
            _isPanelInitialized = true;
        }

        private void KeyPanel_Load(object sender, EventArgs e)
        {
            _initializing = false;
            InitActionComboBox();
            LoadKeySettings();
        }

        /// <summary>
        /// 키 설정 로드
        /// </summary>
        public override void LoadSettings()
        {
            try
            {
                InitializePanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("키 설정 로드 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public override void SaveSettings()
        {
            try
            {
                if (_keySettings != null)
                {
                    _keySettings.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("키 설정 저장 중 오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 키 설정 로드 및 목록 표시
        /// </summary>
        private void LoadKeySettings()
        {
            _keySettings = KeySettings.Load();
            if (_keySettings == null)
            {
                _keySettings = new KeySettings();
            }

            // 리스트뷰 초기화
            listViewKeys.Items.Clear();
            
            // F1~F12 키 추가
            // Settings 리스트에서 키 가져오기
            foreach (var setting in _keySettings.Settings)
            {
                KeyAction action = setting.Action;
                string optionName = "";
                
                if (action == KeyAction.UserExecute)
                {
                    optionName = setting.UserExecuteOptionName;
                }
                
                ListViewItem item = new ListViewItem(setting.Key.ToString());
                item.SubItems.Add(GetActionDisplayName(action));
                item.SubItems.Add(optionName);
                item.Tag = setting.Key;
                
                listViewKeys.Items.Add(item);
            }
            
            // 미등록된 F1~F12 키 추가
            for (Keys k = Keys.F1; k <= Keys.F12; k++)
            {
                bool exists = false;
                foreach (ListViewItem item in listViewKeys.Items)
                {
                    if ((Keys)item.Tag == k)
                    {
                        exists = true;
                        break;
                    }
                }
                
                if (!exists)
                {
                    ListViewItem item = new ListViewItem(k.ToString());
                    item.SubItems.Add(GetActionDisplayName(KeyAction.None));
                    item.SubItems.Add("");
                    item.Tag = k;
                    listViewKeys.Items.Add(item);
                }
            }
            
            // UI 상태 초기화
            // UpdateEditPanel(null);
        }

        /// <summary>
        /// 동작 콤보박스 초기화
        /// </summary>
        private void InitActionComboBox()
        {
            cmbAction.Items.Clear();
            
            // 동작 목록 추가
            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                cmbAction.Items.Add(new ComboBoxItem(GetActionDisplayName(action), action));
            }
            
            if (cmbAction.Items.Count > 0)
                cmbAction.SelectedIndex = 0;
        }

        /// <summary>
        /// 동작 표시 이름 가져오기
        /// </summary>
        private string GetActionDisplayName(KeyAction action)
        {
            switch (action)
            {
                case KeyAction.Copy:
                    return "복사";
                case KeyAction.Cut:
                    return "이동";
                case KeyAction.CreateFolder:
                    return "새 폴더";
                case KeyAction.Delete:
                    return "삭제";
                case KeyAction.Rename:
                    return "이름 변경";
                case KeyAction.Properties:
                    return "속성 보기";
                case KeyAction.Refresh:
                    return "새로 고침";
                case KeyAction.UserExecute:
                    return "사용자 정의 명령 실행";
                case KeyAction.View:
                    return "보기";
                case KeyAction.Edit:
                    return "편집";
                case KeyAction.None:
                default:
                    return "없음";
            }
        }

        /// <summary>
        /// 편집 패널 상태 업데이트
        /// </summary>
        private void UpdateEditPanel(ListViewItem item)
        {
            if (item == null)
            {
                txtSelectedKey.Text = "";
                if (cmbAction.Items.Count > 0)
                    cmbAction.SelectedIndex = 0;
                else
                    cmbAction.SelectedIndex = -1;
                    
                cmbOption.Items.Clear();
                cmbOption.Enabled = false;
                btnApply.Enabled = false;
                btnReset.Enabled = false;
                _selectedItem = null;
                return;
            }
            
            _selectedItem = item;
            Keys key = (Keys)item.Tag;
            KeyAction action = _keySettings.GetActionForKey(key);
            
            // 키 이름 표시
            txtSelectedKey.Text = key.ToString();
            
            // 동작 선택
            for (int i = 0; i < cmbAction.Items.Count; i++)
            {
                ComboBoxItem cbItem = (ComboBoxItem)cmbAction.Items[i];
                if ((KeyAction)cbItem.Value == action)
                {
                    cmbAction.SelectedIndex = i;
                    break;
                }
            }
            
            // 옵션 설정
            UpdateOptionComboBox(action);
            
            // 옵션 선택
            if (action == KeyAction.UserExecute)
            {
                string optionName = _keySettings.GetUserExecuteOptionNameForKey(key);
                if (!string.IsNullOrEmpty(optionName))
                {
                    cmbOption.Text = optionName;
                }
            }
            
            btnApply.Enabled = true;
            btnReset.Enabled = true;
        }

        /// <summary>
        /// 동작에 따라 옵션 콤보박스 업데이트
        /// </summary>
        private void UpdateOptionComboBox(KeyAction action)
        {
            cmbOption.Items.Clear();
            
            if (action == KeyAction.UserExecute)
            {
                cmbOption.Enabled = true;
                
                // 사용자 정의 명령 목록 가져오기
                var options = _keySettings.UserExecuteOptions;
                foreach (var option in options)
                {
                    cmbOption.Items.Add(option.Name);
                }
                
                if (cmbOption.Items.Count > 0)
                    cmbOption.SelectedIndex = 0;
            }
            else
            {
                cmbOption.Enabled = false;
            }
        }

        /// <summary>
        /// 리스트뷰 선택 변경 이벤트
        /// </summary>
        private void listViewKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewKeys.SelectedItems.Count > 0)
            {
                UpdateEditPanel(listViewKeys.SelectedItems[0]);
            }
            else
            {
                UpdateEditPanel(null);
            }
        }

        /// <summary>
        /// 동작 콤보박스 선택 변경 이벤트
        /// </summary>
        private void cmbAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing || cmbAction.SelectedItem == null) return;
            
            ComboBoxItem item = (ComboBoxItem)cmbAction.SelectedItem;
            KeyAction action = (KeyAction)item.Value;
            
            UpdateOptionComboBox(action);
        }

        /// <summary>
        /// 적용 버튼 클릭 이벤트
        /// </summary>
        private void btnApply_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null || cmbAction.SelectedItem == null) return;
            
            Keys key = (Keys)_selectedItem.Tag;
            ComboBoxItem item = (ComboBoxItem)cmbAction.SelectedItem;
            KeyAction action = (KeyAction)item.Value;
            
            // 설정 적용
            // KeySetting 객체 찾기 또는 생성
            KeySetting setting = null;
            foreach (var s in _keySettings.Settings)
            {
                if (s.Key == key)
                {
                    setting = s;
                    break;
                }
            }
            
            if (setting == null)
            {
                setting = new KeySetting(key, action);
                _keySettings.Settings.Add(setting);
            }
            else
            {
                setting.Action = action;
                if (action != KeyAction.UserExecute)
                {
                    setting.UserExecuteOptionName = "";
                }
            }
            
            // 사용자 정의 명령인 경우 옵션 설정
            if (action == KeyAction.UserExecute && cmbOption.SelectedItem != null)
            {
                string optionName = cmbOption.SelectedItem.ToString();
                setting.UserExecuteOptionName = optionName;
            }
            
            // 목록 업데이트
            _selectedItem.SubItems[1].Text = GetActionDisplayName(action);
            _selectedItem.SubItems[2].Text = (action == KeyAction.UserExecute && cmbOption.SelectedItem != null) 
                ? cmbOption.SelectedItem.ToString() : "";
            
            MessageBox.Show("키 설정이 적용되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 초기화 버튼 클릭 이벤트
        /// </summary>
        private void btnReset_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null) return;
            
            if (MessageBox.Show("선택한 키의 설정을 초기화하시겠습니까?", "확인", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Keys key = (Keys)_selectedItem.Tag;
                
                // 기존 설정 제거
                for (int i = 0; i < _keySettings.Settings.Count; i++)
                {
                    if (_keySettings.Settings[i].Key == key)
                    {
                        _keySettings.Settings.RemoveAt(i);
                        break;
                    }
                }
                
                // 목록 업데이트
                KeyAction action = KeyAction.None;
                _selectedItem.SubItems[1].Text = GetActionDisplayName(action);
                _selectedItem.SubItems[2].Text = "";
                
                // 편집 패널 업데이트
                UpdateEditPanel(_selectedItem);
                
                MessageBox.Show("키 설정이 초기화되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
} 