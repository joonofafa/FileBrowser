using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;

namespace TotalCommander.GUI
{
    public partial class FormKeySettings : Form
    {
        private KeySettings keySettings;
        private Dictionary<Keys, ComboBox> keyComboBoxes = new Dictionary<Keys, ComboBox>();
        private Dictionary<Keys, ComboBox> userExecuteOptionComboBoxes = new Dictionary<Keys, ComboBox>();

        public FormKeySettings()
        {
            InitializeComponent();
            keySettings = KeySettings.Load();
        }

        public FormKeySettings(KeySettings settings)
        {
            InitializeComponent();
            keySettings = settings;
        }

        // Designer에서 생성될 코드
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDefault = new System.Windows.Forms.Button();
            this.tblKeySettings = new System.Windows.Forms.TableLayoutPanel();
            this.btnManageUserOptions = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(155, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Function Key Settings";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.Location = new System.Drawing.Point(317, 320);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(398, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnDefault
            // 
            this.btnDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDefault.Location = new System.Drawing.Point(12, 320);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(87, 23);
            this.btnDefault.TabIndex = 4;
            this.btnDefault.Text = "Reset to Default";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // tblKeySettings
            // 
            this.tblKeySettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblKeySettings.ColumnCount = 3;
            this.tblKeySettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblKeySettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tblKeySettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tblKeySettings.Location = new System.Drawing.Point(12, 40);
            this.tblKeySettings.Name = "tblKeySettings";
            this.tblKeySettings.RowCount = 8;
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tblKeySettings.Size = new System.Drawing.Size(461, 260);
            this.tblKeySettings.TabIndex = 5;
            // 
            // btnManageUserOptions
            // 
            this.btnManageUserOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnManageUserOptions.Location = new System.Drawing.Point(105, 320);
            this.btnManageUserOptions.Name = "btnManageUserOptions";
            this.btnManageUserOptions.Size = new System.Drawing.Size(133, 23);
            this.btnManageUserOptions.TabIndex = 6;
            this.btnManageUserOptions.Text = "사용자 실행 옵션 관리...";
            this.btnManageUserOptions.UseVisualStyleBackColor = true;
            this.btnManageUserOptions.Click += new System.EventHandler(this.btnManageUserOptions_Click);
            // 
            // FormKeySettings
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(485, 361);
            this.Controls.Add(this.btnManageUserOptions);
            this.Controls.Add(this.tblKeySettings);
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormKeySettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Function Key Settings";
            this.Load += new System.EventHandler(this.FormKeySettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDefault;
        private System.Windows.Forms.TableLayoutPanel tblKeySettings;
        private System.Windows.Forms.Button btnManageUserOptions;

        private void FormKeySettings_Load(object sender, EventArgs e)
        {
            // 테이블 레이아웃 초기화
            tblKeySettings.Controls.Clear();
            tblKeySettings.RowCount = 8; // 헤더 + F2~F8 키 7개

            // 헤더 추가
            var headerKey = new Label { Text = "Function Key", Font = new Font(Font, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var headerAction = new Label { Text = "Action", Font = new Font(Font, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var headerUserOption = new Label { Text = "User Execute Option", Font = new Font(Font, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            tblKeySettings.Controls.Add(headerKey, 0, 0);
            tblKeySettings.Controls.Add(headerAction, 1, 0);
            tblKeySettings.Controls.Add(headerUserOption, 2, 0);

            // F2~F8 키 설정 UI 추가
            for (int i = 0; i < 7; i++)
            {
                Keys key = Keys.F2 + i;
                int rowIndex = i + 1; // 헤더 다음부터 시작

                // 키 레이블
                var lblKey = new Label { Text = key.ToString(), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
                tblKeySettings.Controls.Add(lblKey, 0, rowIndex);

                // 액션 콤보박스
                var cboAction = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
                PopulateActionComboBox(cboAction);
                
                // 현재 설정된 액션 선택
                KeyAction currentAction = keySettings.GetActionForKey(key);
                cboAction.SelectedItem = GetActionDisplayText(currentAction);
                cboAction.Tag = key; // 나중에 어떤 키에 대한 콤보박스인지 알기 위해
                cboAction.SelectedIndexChanged += CboAction_SelectedIndexChanged;

                tblKeySettings.Controls.Add(cboAction, 1, rowIndex);
                keyComboBoxes[key] = cboAction;

                // 사용자 실행 옵션 콤보박스 (사용자 실행 옵션이 선택된 경우에만 활성화)
                var cboUserOption = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
                PopulateUserExecuteOptionComboBox(cboUserOption);
                
                // 현재 설정된 사용자 실행 옵션 선택
                string currentUserOptionName = keySettings.GetUserExecuteOptionNameForKey(key);
                if (!string.IsNullOrEmpty(currentUserOptionName))
                {
                    cboUserOption.SelectedItem = currentUserOptionName;
                }

                // 기본적으로 비활성화 (UserExecute 액션 선택 시 활성화)
                cboUserOption.Enabled = (currentAction == KeyAction.UserExecute);

                tblKeySettings.Controls.Add(cboUserOption, 2, rowIndex);
                userExecuteOptionComboBoxes[key] = cboUserOption;
            }
        }

        private void CboAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cboAction = sender as ComboBox;
            if (cboAction == null || cboAction.Tag == null)
                return;

            Keys key = (Keys)cboAction.Tag;
            
            // 사용자 실행 옵션 콤보박스 찾기
            if (userExecuteOptionComboBoxes.TryGetValue(key, out ComboBox cboUserOption))
            {
                // 선택된 액션이 UserExecute인 경우에만 활성화
                string selectedActionText = cboAction.SelectedItem?.ToString() ?? "";
                KeyAction selectedAction = GetActionFromDisplayText(selectedActionText);
                
                cboUserOption.Enabled = (selectedAction == KeyAction.UserExecute);
                
                // 사용자 실행 옵션이 선택되지 않았고 옵션 목록이 있는 경우 첫 번째 항목 선택
                if (selectedAction == KeyAction.UserExecute && 
                    (cboUserOption.SelectedIndex < 0 || cboUserOption.SelectedItem == null) && 
                    cboUserOption.Items.Count > 0)
                {
                    cboUserOption.SelectedIndex = 0;
                }
            }
        }

        private void PopulateActionComboBox(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            
            // KeyAction enum의 모든 값을 콤보박스에 추가
            foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
            {
                comboBox.Items.Add(GetActionDisplayText(action));
            }
        }

        private void PopulateUserExecuteOptionComboBox(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            
            // 사용자 실행 옵션 목록 추가
            foreach (var option in keySettings.UserExecuteOptions)
            {
                comboBox.Items.Add(option.Name);
            }
        }

        private string GetActionDisplayText(KeyAction action)
        {
            switch (action)
            {
                case KeyAction.None: return "None";
                case KeyAction.View: return "View File";
                case KeyAction.Edit: return "Edit File";
                case KeyAction.Copy: return "Copy";
                case KeyAction.Cut: return "Cut";
                case KeyAction.Paste: return "Paste";
                case KeyAction.Delete: return "Delete";
                case KeyAction.CreateFolder: return "Create Folder";
                case KeyAction.Properties: return "Properties";
                case KeyAction.Refresh: return "Refresh";
                case KeyAction.GoParent: return "Go to Parent";
                case KeyAction.GoBack: return "Go Back";
                case KeyAction.GoForward: return "Go Forward";
                case KeyAction.Exit: return "Exit";
                case KeyAction.UserExecute: return "User Execute Option";
                default: return action.ToString();
            }
        }

        private KeyAction GetActionFromDisplayText(string displayText)
        {
            if (displayText == "None") return KeyAction.None;
            if (displayText == "View File") return KeyAction.View;
            if (displayText == "Edit File") return KeyAction.Edit;
            if (displayText == "Copy") return KeyAction.Copy;
            if (displayText == "Cut") return KeyAction.Cut;
            if (displayText == "Paste") return KeyAction.Paste;
            if (displayText == "Delete") return KeyAction.Delete;
            if (displayText == "Create Folder") return KeyAction.CreateFolder;
            if (displayText == "Properties") return KeyAction.Properties;
            if (displayText == "Refresh") return KeyAction.Refresh;
            if (displayText == "Go to Parent") return KeyAction.GoParent;
            if (displayText == "Go Back") return KeyAction.GoBack;
            if (displayText == "Go Forward") return KeyAction.GoForward;
            if (displayText == "Exit") return KeyAction.Exit;
            if (displayText == "User Execute Option") return KeyAction.UserExecute;

            // 기본값
            return KeyAction.None;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 사용자에게 저장 확인 메시지 표시
            if (MessageBox.Show("이 설정을 저장하시겠습니까? 저장 후에는 즉시 적용됩니다.", 
                              "설정 저장 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return; // 사용자가 취소함
            }

            // 콤보박스에서 선택된 설정을 keySettings 객체에 적용
            keySettings.Settings.Clear();

            foreach (var keyPair in keyComboBoxes)
            {
                Keys key = keyPair.Key;
                ComboBox comboBox = keyPair.Value;
                string selectedText = comboBox.SelectedItem?.ToString() ?? "None";
                KeyAction action = GetActionFromDisplayText(selectedText);

                // 사용자 실행 옵션을 위한 추가 설정
                string userExecuteOptionName = "";
                if (action == KeyAction.UserExecute && userExecuteOptionComboBoxes.TryGetValue(key, out ComboBox cboUserOption))
                {
                    userExecuteOptionName = cboUserOption.SelectedItem?.ToString() ?? "";
                }

                // 디버그: F5 키 설정 확인
                if (key == Keys.F5)
                {
                    string debugMsg = $"F5 키 설정 저장: 액션={action}";
                    if (action == KeyAction.UserExecute)
                        debugMsg += $", 실행 옵션={userExecuteOptionName}";
                    MessageBox.Show(debugMsg, "F5 키 설정 저장", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // 설정 추가
                keySettings.Settings.Add(new KeySetting(key, action, userExecuteOptionName));
            }

            // 설정 저장 - 저장 오류 가능성 확인
            try
            {
                keySettings.Save();
                
                // 설정 파일이 실제로 저장되었는지 확인
                string settingsPath = KeySettings.GetSettingsFilePath();
                if (File.Exists(settingsPath))
                {
                    // 저장된 파일 다시 읽어서 F5 키 설정 확인 (새로운 인스턴스로 로드)
                    KeySettings loadedSettings = new KeySettings();
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(KeySettings));
                        using (StreamReader reader = new StreamReader(settingsPath))
                        {
                            loadedSettings = (KeySettings)serializer.Deserialize(reader);
                        }
                        
                        KeyAction loadedF5Action = KeyAction.None;
                        string loadedF5Option = "";
                        
                        foreach (var setting in loadedSettings.Settings)
                        {
                            if (setting.Key == Keys.F5)
                            {
                                loadedF5Action = setting.Action;
                                loadedF5Option = setting.UserExecuteOptionName;
                                break;
                            }
                        }
                        
                        string verifyMsg = $"설정 파일 검증: F5 키 액션={loadedF5Action}";
                        if (loadedF5Action == KeyAction.UserExecute && !string.IsNullOrEmpty(loadedF5Option))
                            verifyMsg += $", 실행 옵션={loadedF5Option}";
                        
                        MessageBox.Show(verifyMsg, "설정 저장 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("저장된 설정 파일 검증 중 오류: " + ex.Message,
                                      "설정 검증 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("설정 파일이 저장되지 않았습니다: " + settingsPath, 
                                  "설정 저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("설정을 저장하는 중 오류가 발생했습니다: " + ex.Message, 
                               "설정 저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            // 메인 폼에 변경 사항이 즉시 적용되도록 해당 폼을 찾아 설정을 다시 로드
            Form mainForm = this.Owner;
            if (mainForm != null && mainForm is Form_TotalCommander)
            {
                // 리플렉션을 사용하여 keySettings 필드 접근
                try
                {
                    System.Reflection.FieldInfo fieldInfo = mainForm.GetType().GetField("keySettings", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (fieldInfo != null)
                    {
                        // 직접 필드 값 설정 - 새로 로드하지 않고 현재 설정 그대로 사용
                        fieldInfo.SetValue(mainForm, keySettings);
                        
                        // 변경 사항 적용 확인 메시지
                        MessageBox.Show("키 설정이 저장되고 메인 폼에 적용되었습니다.", 
                                      "설정 적용 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("설정을 메인 폼에 적용하는 중 오류가 발생했습니다: " + ex.Message,
                                  "설정 적용 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            DialogResult = DialogResult.OK;
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            // 기본 설정으로 초기화
            keySettings.SetDefaults();

            // UI 업데이트
            foreach (var keyPair in keyComboBoxes)
            {
                Keys key = keyPair.Key;
                ComboBox comboBox = keyPair.Value;
                KeyAction action = keySettings.GetActionForKey(key);
                comboBox.SelectedItem = GetActionDisplayText(action);

                // 사용자 실행 옵션 콤보박스 비활성화
                if (userExecuteOptionComboBoxes.TryGetValue(key, out ComboBox cboUserOption))
                {
                    cboUserOption.Enabled = false;
                    cboUserOption.SelectedIndex = -1;
                }
            }
        }

        private void btnManageUserOptions_Click(object sender, EventArgs e)
        {
            // 사용자 실행 옵션 관리 폼 표시
            using (FormManageUserOptions form = new FormManageUserOptions(keySettings))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // 옵션 목록이 변경되었으므로 콤보박스 업데이트
                    foreach (var keyPair in userExecuteOptionComboBoxes)
                    {
                        ComboBox comboBox = keyPair.Value;
                        string selectedOption = comboBox.SelectedItem?.ToString() ?? "";
                        
                        PopulateUserExecuteOptionComboBox(comboBox);
                        
                        // 이전에 선택된 옵션이 아직 존재하면 다시 선택
                        if (!string.IsNullOrEmpty(selectedOption) && comboBox.Items.Contains(selectedOption))
                        {
                            comboBox.SelectedItem = selectedOption;
                        }
                        else if (comboBox.Enabled && comboBox.Items.Count > 0)
                        {
                            comboBox.SelectedIndex = 0;
                        }
                    }
                }
            }
        }
    }
} 