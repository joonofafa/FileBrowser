using System;
using TotalCommander.GUI;

namespace TotalCommander
{
    /// <summary>
    /// FormFontSettings 클래스에 대한 확장 메서드
    /// </summary>
    public static class FormFontSettingsExtensions
    {
        /// <summary>
        /// ApplyToStatusBar 확장 메서드
        /// </summary>
        public static bool ApplyToStatusBar(this FormFontSettings fontSettings)
        {
            // 항상 true를 반환
            return true;
        }
    }
} 