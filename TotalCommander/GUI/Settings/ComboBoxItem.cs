using System;

namespace TotalCommander.GUI.Settings
{
    /// <summary>
    /// 콤보박스 아이템을 표현하는 클래스
    /// </summary>
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        
        public ComboBoxItem()
        {
            Text = "";
            Value = null;
        }
        
        public ComboBoxItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }
} 