using System;
using System.Windows.Forms;

namespace TotalCommander.GUI.Settings
{
    /// <summary>
    /// 설정 패널의 공통 인터페이스
    /// </summary>
    public interface ISettingsPanel
    {
        /// <summary>
        /// 패널 컨트롤 가져오기
        /// </summary>
        UserControl PanelControl { get; }

        /// <summary>
        /// 패널 이름 가져오기
        /// </summary>
        string PanelName { get; }

        /// <summary>
        /// 메인폼 설정
        /// </summary>
        void SetMainForm(Form_TotalCommander mainForm);

        /// <summary>
        /// 현재 설정 로드
        /// </summary>
        void LoadSettings();

        /// <summary>
        /// 설정 저장
        /// </summary>
        void SaveSettings();
    }
} 