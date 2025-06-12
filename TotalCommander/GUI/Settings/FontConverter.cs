using System;
using System.Drawing;

namespace TotalCommander.GUI.Settings
{
    /// <summary>
    /// 글꼴 변환 유틸리티
    /// </summary>
    public static class FontConverter
    {
        /// <summary>
        /// 글꼴을 문자열로 변환
        /// </summary>
        public static string ToString(Font font)
        {
            if (font == null) return string.Empty;
            
            return $"{font.FontFamily.Name};{font.Size};{(int)font.Style}";
        }

        /// <summary>
        /// 문자열에서 글꼴로 변환
        /// </summary>
        public static Font FromString(string fontString)
        {
            if (string.IsNullOrEmpty(fontString)) 
                return new Font("굴림", 9);
            
            string[] parts = fontString.Split(';');
            if (parts.Length != 3) 
                return new Font("굴림", 9);
            
            try
            {
                string name = parts[0];
                float size = float.Parse(parts[1]);
                FontStyle style = (FontStyle)int.Parse(parts[2]);
                
                return new Font(name, size, style);
            }
            catch
            {
                return new Font("굴림", 9);
            }
        }
    }
}
