using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace WPFFloatWin
{
    class TagText:TagBase
    {
        string _text_data;
        TextBox _text_input;
        int _line_count;
        public TagText(ref TagWindow host):base(ref host)
        {
            _text_data = "";
            _line_count = -1;
        }
        public override string GetName()
        {
            return "Text";
        }

        public override bool CanCreateFromFile
        {
            get { return false; }
        }
        public override bool CanCreateNewFile
        {
            get { return true; }
        }
        public override bool RightClickActive
        {
            get { return true; }
        }
        public override void OnCreateNew()
        {
            base.OnCreateNew();
            IsCreated = true;
        }
        public override void OnInit()
        {
            base.OnInit();
            _text_input = new TextBox();
            _text_input.Height = window.tw_content.ActualHeight;
            _text_input.Width = window.tw_content.ActualWidth;
            _text_input.Background = null;
            _text_input.BorderBrush = null;
            TextBlock.SetLineHeight(_text_input, _text_input.Height);
            _text_input.Foreground = Brushes.White;
            _text_input.Text = _text_data;
            _text_input.FontFamily= new FontFamily("Agency FB");
            _text_input.VerticalContentAlignment = VerticalAlignment.Top;
            _text_input.HorizontalContentAlignment = HorizontalAlignment.Left;
            _text_input.FontSize = 30;
            _text_input.TextWrapping = TextWrapping.Wrap;
            _text_input.AcceptsReturn = true;
            _text_input.LayoutUpdated += (s, e) => { TextInputEvent(); };
        }
        private void TextInputEvent()
        {
            if (_line_count != _text_input.LineCount)
            {
                int lines = _text_input.LineCount;
                int lineheight = (int)TextBlock.GetLineHeight(_text_input);
                Height = TagWindow.CtrlButtonSize + (lines - 1) * lineheight;
                _text_input.Height = (lines) * lineheight;
                _line_count = lines;
            }
        }
        public override void OnTransfer()
        {
            base.OnTransfer();
            _line_count = -1;
        }
        public override void OnShow()
        {
            base.OnShow();
            window.tw_content.Children.Add(_text_input);
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override string OnSave()
        {
            base.OnSave();
            string result = Regex.Escape(_text_input.Text);
            return result;
        }
        public override void OnLoad(string data)
        {
            base.OnLoad(data);
            _text_data = data;
            IsCreated = true;
        }
        
    }
}
