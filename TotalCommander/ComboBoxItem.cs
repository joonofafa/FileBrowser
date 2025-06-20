using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TotalCommander;

namespace TotalCommander
{
    /// <summary>
    /// Represents an item in System.Windows.Forms.ComboBox
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

