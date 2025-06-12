using System;
using System.Windows.Forms;

namespace TotalCommander.GUI.Settings
{
    /// <summary>
    /// 설정 패널의 기본 UserControl 클래스
    /// </summary>
    public partial class SettingsPanelBase : UserControl, ISettingsPanel
    {
        private string _panelName;
        private Form_TotalCommander _mainForm;
        
        /// <summary>
        /// 기본 생성자
        /// </summary>
        public SettingsPanelBase()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.Visible = false;
            _panelName = "";
        }
        
        /// <summary>
        /// 메인폼 설정
        /// </summary>
        /// <param name="mainForm">메인 폼</param>
        public void SetMainForm(Form_TotalCommander mainForm)
        {
            _mainForm = mainForm;
        }
        
        /// <summary>
        /// 패널 이름 설정
        /// </summary>
        /// <param name="name">패널 이름</param>
        protected void SetPanelName(string name)
        {
            _panelName = name;
        }
        
        /// <summary>
        /// 패널 컨트롤 가져오기
        /// </summary>
        public UserControl PanelControl => this;
        
        /// <summary>
        /// 패널 이름 가져오기
        /// </summary>
        public string PanelName => _panelName;
        
        /// <summary>
        /// 메인폼 가져오기
        /// </summary>
        protected Form_TotalCommander GetMainForm()
        {
            return _mainForm;
        }
        
        /// <summary>
        /// 현재 설정 로드 (자식 클래스에서 구현)
        /// </summary>
        public virtual void LoadSettings()
        {
        }
        
        /// <summary>
        /// 설정 저장 (자식 클래스에서 구현)
        /// </summary>
        public virtual void SaveSettings()
        {
        }
    }
} 