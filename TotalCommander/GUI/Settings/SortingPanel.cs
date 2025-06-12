using System;
using System.Drawing;
using System.Windows.Forms;

namespace TotalCommander.GUI.Settings
{
    public partial class SortingPanel : SettingsPanelBase
    {
        public SortingPanel()
        {
            InitializeComponent();
            SetPanelName("정렬");
        }

        /// <summary>
        /// 현재 설정 로드
        /// </summary>
        public override void LoadSettings()
        {
            // 설정에서 정렬 방식 로드
            string sortMode = Properties.Settings.Default.SortMode;
            bool sortReverse = Properties.Settings.Default.SortReverse;
            bool dirsFirst = Properties.Settings.Default.DirsFirst;

            switch (sortMode)
            {
                case "Name":
                    radioSortByName.Checked = true;
                    break;
                case "Extension":
                    radioSortByExt.Checked = true;
                    break;
                case "Size":
                    radioSortBySize.Checked = true;
                    break;
                case "Date":
                    radioSortByDate.Checked = true;
                    break;
                default:
                    radioSortByName.Checked = true;
                    break;
            }

            checkReverse.Checked = sortReverse;
            checkDirsFirst.Checked = dirsFirst;
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public override void SaveSettings()
        {
            // 정렬 설정 저장
            string sortMode = "Name";
            
            if (radioSortByName.Checked)
                sortMode = "Name";
            else if (radioSortByExt.Checked)
                sortMode = "Extension";
            else if (radioSortBySize.Checked)
                sortMode = "Size";
            else if (radioSortByDate.Checked)
                sortMode = "Date";
            
            Properties.Settings.Default.SortMode = sortMode;
            Properties.Settings.Default.SortReverse = checkReverse.Checked;
            Properties.Settings.Default.DirsFirst = checkDirsFirst.Checked;
        }
    }
}
