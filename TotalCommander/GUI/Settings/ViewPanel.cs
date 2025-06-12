using System;
using System.Drawing;
using System.Windows.Forms;

namespace TotalCommander.GUI.Settings
{
    public partial class ViewPanel : SettingsPanelBase
    {
        public ViewPanel()
        {
            InitializeComponent();
            SetPanelName("보기");
        }

        /// <summary>
        /// 현재 설정 로드
        /// </summary>
        public override void LoadSettings()
        {
            checkShowHidden.Checked = Properties.Settings.Default.ShowHiddenFiles;
            checkShowSystem.Checked = Properties.Settings.Default.ShowSystemFiles;
            checkFullRowSelect.Checked = Properties.Settings.Default.FullRowSelect;
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public override void SaveSettings()
        {
            Properties.Settings.Default.ShowHiddenFiles = checkShowHidden.Checked;
            Properties.Settings.Default.ShowSystemFiles = checkShowSystem.Checked;
            Properties.Settings.Default.FullRowSelect = checkFullRowSelect.Checked;
        }
    }
}
