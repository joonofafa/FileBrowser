using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TotalCommander.GUI.Settings;

namespace TotalCommander.GUI
{
    /// <summary>
    /// 통합 설정 대화상자 (FileZilla 스타일)
    /// </summary>
    public partial class FormSettingsNew : Form
    {
        private Form_TotalCommander _mainForm;
        private Font _treeFont;
        private Dictionary<string, ISettingsPanel> _settingsPanels = new Dictionary<string, ISettingsPanel>();

        public FormSettingsNew(Form_TotalCommander mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            _treeFont = new Font("굴림", 9);
            this.Font = _treeFont;
        }

        #region UI 초기화
        private void InitializeComponent()
        {
            this.splitter = new System.Windows.Forms.SplitContainer();
            this.treeSettings = new System.Windows.Forms.TreeView();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
            this.splitter.Panel1.SuspendLayout();
            this.splitter.Panel2.SuspendLayout();
            this.splitter.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitter
            // 
            this.splitter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitter.Location = new System.Drawing.Point(12, 12);
            this.splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            this.splitter.Panel1.Controls.Add(this.treeSettings);
            this.splitter.Panel1MinSize = 180;
            // 
            // splitter.Panel2
            // 
            this.splitter.Panel2.Controls.Add(this.panelContainer);
            this.splitter.Size = new System.Drawing.Size(710, 438);
            this.splitter.SplitterDistance = 200;
            this.splitter.TabIndex = 0;
            // 
            // treeSettings
            // 
            this.treeSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeSettings.HideSelection = false;
            this.treeSettings.Location = new System.Drawing.Point(0, 0);
            this.treeSettings.Name = "treeSettings";
            this.treeSettings.Size = new System.Drawing.Size(200, 438);
            this.treeSettings.TabIndex = 0;
            this.treeSettings.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeSettings_AfterSelect);
            // 
            // panelContainer
            // 
            this.panelContainer.BackColor = System.Drawing.SystemColors.Control;
            this.panelContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContainer.Location = new System.Drawing.Point(0, 0);
            this.panelContainer.Name = "panelContainer";
            this.panelContainer.Size = new System.Drawing.Size(506, 438);
            this.panelContainer.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(462, 456);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 30);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "확인";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(548, 456);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "취소";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(642, 456);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(80, 30);
            this.btnApply.TabIndex = 3;
            this.btnApply.Text = "적용";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // FormSettingsNew
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(734, 498);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.splitter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettingsNew";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "설정";
            this.Load += new System.EventHandler(this.FormSettingsNew_Load);
            this.splitter.Panel1.ResumeLayout(false);
            this.splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitter)).EndInit();
            this.splitter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        #region 컨트롤 변수
        private System.Windows.Forms.SplitContainer splitter;
        private System.Windows.Forms.TreeView treeSettings;
        private System.Windows.Forms.Panel panelContainer;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        #endregion

        #region 이벤트 핸들러
        private void FormSettingsNew_Load(object sender, EventArgs e)
        {
            InitializeSettingsTree();
            InitializeSettingsPanels();
            LoadCurrentSettings();
            
            // 트리뷰 첫 번째 항목 선택
            if (treeSettings.Nodes.Count > 0)
            {
                treeSettings.SelectedNode = treeSettings.Nodes[0];
                if (treeSettings.Nodes[0].Nodes.Count > 0)
                {
                    treeSettings.SelectedNode = treeSettings.Nodes[0].Nodes[0];
                }
            }
        }

        private void treeSettings_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowSettingsPanel(e.Node);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            SaveSettings();
            btnApply.Enabled = false;
        }
        #endregion

        #region 설정 초기화 및 로드/저장
        private void InitializeSettingsTree()
        {
            treeSettings.Nodes.Clear();
            treeSettings.Font = _treeFont;

            // 트리 노드 생성
            TreeNode generalNode = treeSettings.Nodes.Add("일반");
            generalNode.Nodes.Add("정렬");
            generalNode.Nodes.Add("보기");
            generalNode.Nodes.Add("언어");
            
            TreeNode appearanceNode = treeSettings.Nodes.Add("모양");
            appearanceNode.Nodes.Add("글꼴");
            appearanceNode.Nodes.Add("색상");
            appearanceNode.Nodes.Add("도구 모음");
            
            TreeNode transferNode = treeSettings.Nodes.Add("파일 전송");
            transferNode.Nodes.Add("복사/이동");
            transferNode.Nodes.Add("압축");
            
            TreeNode connectionNode = treeSettings.Nodes.Add("연결");
            connectionNode.Nodes.Add("FTP");
            connectionNode.Nodes.Add("SSH");
            
            // 기능키 관련 노드 추가
            TreeNode keyboardNode = treeSettings.Nodes.Add("단축키");
            keyboardNode.Nodes.Add("기능 키");
            keyboardNode.Nodes.Add("사용자 명령");
            
            treeSettings.ExpandAll();
        }

        private void InitializeSettingsPanels()
        {
            // UserControl 기반 패널 생성
            var sortingPanel = new SortingPanel();
            var viewPanel = new ViewPanel();
            var fontPanel = new FontPanel();
            var keyPanel = new KeyPanel();
            var userCommandPanel = new UserCommandPanel();
            
            // 메인폼 설정
            sortingPanel.SetMainForm(_mainForm);
            viewPanel.SetMainForm(_mainForm);
            fontPanel.SetMainForm(_mainForm);
            keyPanel.SetMainForm(_mainForm);
            userCommandPanel.SetMainForm(_mainForm);
            
            // 패널 컨테이너에 추가
            panelContainer.Controls.Add(sortingPanel);
            panelContainer.Controls.Add(viewPanel);
            panelContainer.Controls.Add(fontPanel);
            panelContainer.Controls.Add(keyPanel);
            panelContainer.Controls.Add(userCommandPanel);
            
            // 패널 딕셔너리에 추가
            _settingsPanels.Add(sortingPanel.PanelName, sortingPanel);
            _settingsPanels.Add(viewPanel.PanelName, viewPanel);
            _settingsPanels.Add(fontPanel.PanelName, fontPanel);
            _settingsPanels.Add(keyPanel.PanelName, keyPanel);
            _settingsPanels.Add(userCommandPanel.PanelName, userCommandPanel);
            
            // 나머지 패널은 필요에 따라 추가로 구현할 수 있음
        }

        private void ShowSettingsPanel(TreeNode node)
        {
            if (node == null) return;
            
            string key = node.Text;
            
            // 모든 패널 숨기기
            foreach (Control ctrl in panelContainer.Controls)
            {
                ctrl.Visible = false;
            }
            
            // 선택된 패널 표시
            if (_settingsPanels.ContainsKey(key))
            {
                _settingsPanels[key].PanelControl.Visible = true;
            }
            else
            {
                // 구현되지 않은 메뉴 항목 처리
                // 임시 라벨 생성 및 표시
                Label label = new Label
                {
                    Text = "이 기능은 아직 구현되지 않았습니다.",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold)
                };
                
                // 기존 임시 라벨 제거
                foreach (Control ctrl in panelContainer.Controls)
                {
                    if (ctrl is Label tempLabel && tempLabel.Tag != null && tempLabel.Tag.ToString() == "temp")
                    {
                        panelContainer.Controls.Remove(tempLabel);
                        tempLabel.Dispose();
                    }
                }
                
                label.Tag = "temp";
                panelContainer.Controls.Add(label);
                label.BringToFront();
            }
        }

        private void LoadCurrentSettings()
        {
            // 각 패널의 설정 로드 메서드 호출
            foreach (var panel in _settingsPanels.Values)
            {
                panel.LoadSettings();
            }
        }

        private void SaveSettings()
        {
            // 각 패널의 설정 저장 메서드 호출
            foreach (var panel in _settingsPanels.Values)
            {
                panel.SaveSettings();
            }
            
            // 설정 저장
            Properties.Settings.Default.Save();
            
            // 메인 폼에 설정 변경 알림
            _mainForm.SettingsChanged();
        }
        #endregion
    }
} 